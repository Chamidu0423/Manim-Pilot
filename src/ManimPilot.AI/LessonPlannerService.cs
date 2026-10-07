using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ManimPilot.Core.Models.Lesson;
using ManimPilot.Math;

namespace ManimPilot.AI;

public class GeminiGenerationOptions
{
    public string ApiKey { get; set; } = string.Empty;
    public string ModelName { get; set; } = "gemini-2.0-flash";
    public double Temperature { get; set; } = 0.2; // Low temperature for factual fidelity
}

public interface ILessonPlannerService
{
    Task<LessonModel> GenerateBinaryLessonAsync(
        string binaryString,
        string languageCode = "si-LK",
        GeminiGenerationOptions? options = null,
        CancellationToken ct = default
    );
}

public class LessonPlannerService : ILessonPlannerService
{
    private readonly IBinaryMathService _mathService;
    private readonly HttpClient _httpClient;

    public LessonPlannerService(IBinaryMathService? mathService = null, HttpClient? httpClient = null)
    {
        _mathService = mathService ?? new BinaryMathService();
        _httpClient = httpClient ?? new HttpClient();
    }

    public async Task<LessonModel> GenerateBinaryLessonAsync(
        string binaryString,
        string languageCode = "si-LK",
        GeminiGenerationOptions? options = null,
        CancellationToken ct = default)
    {
        // 1. First, calculate the true mathematical proof deterministically
        var mathResult = _mathService.ConvertBinaryToDecimal(binaryString);
        if (!mathResult.IsValid)
        {
            throw new ArgumentException($"Invalid binary string: {mathResult.ErrorMessage}");
        }

        // 2. Check if Gemini API is configured
        if (options != null && !string.IsNullOrWhiteSpace(options.ApiKey))
        {
            try
            {
                var lessonFromAi = await QueryGeminiForLessonAsync(mathResult, languageCode, options, ct);
                if (lessonFromAi != null)
                {
                    return lessonFromAi;
                }
            }
            catch
            {
                // Fall back to rule-based generation below
            }
        }

        // 3. Deterministic template generation (Guaranteed zero hallucination, perfect Sinhala phrasing)
        return CreateCanonicalBinaryLesson(mathResult, languageCode);
    }

    private LessonModel CreateCanonicalBinaryLesson(BinaryToDecimalResult mathResult, string languageCode)
    {
        string binary = mathResult.NormalizedBinary;
        long decimalResult = mathResult.DecimalResult;
        var placeValues = mathResult.BitsFromLeftToRight.Select(b => b.PlaceValue).ToList();
        var activePlaceValues = mathResult.ActivePlaceValues;
        string additionExpr = mathResult.AdditionExpression;

        bool isSinhala = languageCode.StartsWith("si", StringComparison.OrdinalIgnoreCase);

        var lesson = new LessonModel
        {
            Title = isSinhala ? $"ද්විමය සංඛ්‍යා දශම බවට හැරවීම ({binary})" : $"Binary to Decimal Conversion ({binary})",
            Topic = "BinaryToDecimal",
            Language = languageCode,
            Description = isSinhala
                ? $"{binary} ද්විමය සංඛ්‍යාව ස්ථානීය අගයන් භාවිතයෙන් {decimalResult} දශම සංඛ්‍යාව බවට පත් කරන ආකාරය."
                : $"Step by step conversion of binary {binary} into decimal {decimalResult}.",
            Settings = new LessonSettings
            {
                BackgroundColor = "#0D1117",
                PrimaryColor = "#58A6FF",
                AccentColor = "#7EE787",
                TextColor = "#F0F6FC"
            }
        };

        // Step 1: Show Binary Number
        lesson.Steps.Add(new LessonStepModel
        {
            Id = "step_01",
            OrderIndex = 1,
            Title = isSinhala ? "ද්විමය සංඛ්‍යාව ඉදිරිපත් කිරීම" : "Presenting the Binary Number",
            Type = "show_binary",
            Duration = 3.5,
            Visual = new VisualSpec
            {
                Binary = binary
            },
            Narration = new NarrationSpec
            {
                Text = isSinhala
                    ? $"අපිට දීලා තියෙන්නේ {binary} කියන binary සංඛ්‍යාවයි. අපි දැන් මෙය දශම සංඛ්‍යාවක් බවට හරවමු."
                    : $"We are given the binary number {binary}. Let us convert this into its decimal equivalent.",
                SpokenText = isSinhala
                    ? $"අපිට දීලා තියෙන්නේ {SinhalaNaturalizer.BinaryToSpokenSinhala(binary)} කියන ද්විමය සංඛ්‍යාවයි."
                    : null,
                Anchor = "step_01_binary_shown",
                Timing = TimingRelation.AfterEvent,
                OffsetSeconds = 0.3
            }
        });

        // Step 2: Show Place Values
        lesson.Steps.Add(new LessonStepModel
        {
            Id = "step_02",
            OrderIndex = 2,
            Title = isSinhala ? "ස්ථානීය අගයන් දැක්වීම" : "Displaying Place Values",
            Type = "show_place_values",
            Duration = 4.0,
            Visual = new VisualSpec
            {
                Binary = binary,
                PlaceValues = placeValues
            },
            Narration = new NarrationSpec
            {
                Text = isSinhala
                    ? "දකුණේ සිට වමට දෙකෙහි බලයන් අනුව ස්ථානීය අගයන් පිළිවෙළින් ලියා ගනිමු."
                    : "Writing down the place values from right to left based on powers of two.",
                SpokenText = isSinhala
                    ? "දකුණේ සිට වමට දෙකේ බලයන් අනුව ස්ථානීය අගයන් පිළිවෙළින් සටහන් කරමු."
                    : null,
                Anchor = "step_02_pv_shown",
                Timing = TimingRelation.AfterEvent,
                OffsetSeconds = 0.3
            }
        });

        // Step 3: Select Active Bits
        var activeIndices = new List<int>();
        for (int i = 0; i < binary.Length; i++)
        {
            if (binary[i] == '1') activeIndices.Add(i);
        }

        lesson.Steps.Add(new LessonStepModel
        {
            Id = "step_03",
            OrderIndex = 3,
            Title = isSinhala ? "අගය එක වන ස්ථාන තෝරාගැනීම" : "Selecting Active Bits (Value 1)",
            Type = "select_bits",
            Duration = 4.0,
            Visual = new VisualSpec
            {
                Binary = binary,
                PlaceValues = placeValues,
                ActiveIndices = activeIndices,
                HighlightColor = "#7EE787"
            },
            Narration = new NarrationSpec
            {
                Text = isSinhala
                    ? "මෙහි අගය එක වන ස්ථානවලට අදාළ ස්ථානීය අගයන් පමණක් අපි එකතු කිරීමට තෝරාගන්නවා."
                    : "We select only the place values where the corresponding bit is 1.",
                SpokenText = isSinhala
                    ? "මෙහි අගය එක වන ස්ථානවලට අදාළ ස්ථානීය අගයන් පමණක් අපි එකතු කිරීම සඳහා වෙන් කරගන්නවා."
                    : null,
                Anchor = "step_03_active_highlight",
                Timing = TimingRelation.AfterEvent,
                OffsetSeconds = 0.3
            }
        });

        // Step 4: Addition Expression
        lesson.Steps.Add(new LessonStepModel
        {
            Id = "step_04",
            OrderIndex = 4,
            Title = isSinhala ? "එකතු කිරීමේ ප්‍රකාශනය" : "Addition Expression",
            Type = "addition_expression",
            Duration = 4.5,
            Visual = new VisualSpec
            {
                Binary = binary,
                Expression = additionExpr
            },
            Narration = new NarrationSpec
            {
                Text = isSinhala
                    ? $"දැන් තෝරාගත් අගයන් වන {additionExpr} එකතු කරමු."
                    : $"Now let us sum the active place values: {additionExpr}.",
                SpokenText = isSinhala
                    ? SinhalaNaturalizer.NaturalizeEquationToSpokenSinhala(activePlaceValues, decimalResult)
                    : null,
                Anchor = "step_04_expr_shown",
                Timing = TimingRelation.AfterEvent,
                OffsetSeconds = 0.3
            }
        });

        // Step 5: Final Result
        lesson.Steps.Add(new LessonStepModel
        {
            Id = "step_05",
            OrderIndex = 5,
            Title = isSinhala ? "අවසාන පිළිතුර" : "Final Decimal Result",
            Type = "final_answer",
            Duration = 4.0,
            Visual = new VisualSpec
            {
                Binary = binary,
                Expression = additionExpr,
                Result = decimalResult.ToString()
            },
            Narration = new NarrationSpec
            {
                Text = isSinhala
                    ? $"එමගින් අපට අවසාන පිළිතුර ලෙස {decimalResult} ලැබෙනවා."
                    : $"Thus, the final decimal value is {decimalResult}.",
                SpokenText = isSinhala
                    ? $"එමගින් අපට අවසාන පිළිතුර ලෙස {SinhalaNaturalizer.NumberToSinhalaWords(decimalResult)} ලැබෙනවා."
                    : null,
                Anchor = "step_05_answer_highlight",
                Timing = TimingRelation.AfterEvent,
                OffsetSeconds = 0.3
            }
        });

        return lesson;
    }

    private async Task<LessonModel?> QueryGeminiForLessonAsync(
        BinaryToDecimalResult mathResult,
        string languageCode,
        GeminiGenerationOptions options,
        CancellationToken ct)
    {
        var url = $"https://generativelanguage.googleapis.com/v1beta/models/{options.ModelName}:generateContent?key={options.ApiKey}";

        var prompt = $@"You are the Lesson Planner for ManimPilot Studio.
Create a structured mathematical educational lesson to convert binary '{mathResult.NormalizedBinary}' into decimal '{mathResult.DecimalResult}'.
Language: {languageCode}.
Verified Place Values: [{string.Join(", ", mathResult.BitsFromLeftToRight.Select(b => b.PlaceValue))}].
Active Place Values: [{string.Join(", ", mathResult.ActivePlaceValues)}].
Addition: {mathResult.AdditionExpression} = {mathResult.DecimalResult}.

Respond ONLY with valid JSON conforming to ManimPilot LessonModel schema.";

        var requestBody = new
        {
            contents = new[]
            {
                new { parts = new[] { new { text = prompt } } }
            },
            generationConfig = new
            {
                temperature = options.Temperature,
                response_mime_type = "application/json"
            }
        };

        var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(url, content, ct);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var json = await response.Content.ReadAsStringAsync(ct);
        using var doc = JsonDocument.Parse(json);
        var text = doc.RootElement.GetProperty("candidates")[0].GetProperty("content").GetProperty("parts")[0].GetProperty("text").GetString();

        if (string.IsNullOrEmpty(text)) return null;
        return JsonSerializer.Deserialize<LessonModel>(text);
    }
}
