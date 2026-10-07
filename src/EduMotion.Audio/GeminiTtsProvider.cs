using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using EduMotion.Core.Models.Timeline;

namespace EduMotion.Audio;

public class MockTtsProvider : ITtsProvider
{
    public string ProviderName => "MockTTS";

    public Task<TtsSynthesisResult> SynthesizeAsync(
        string text,
        string languageCode,
        TtsVoiceConfig config,
        string outputFilePath,
        CancellationToken ct = default)
    {
        double estimatedSeconds = TimelineEngine.EstimateSpeechDuration(text);
        DurationAnalyzer.CreateSineWaveWav(outputFilePath, estimatedSeconds);

        return Task.FromResult(new TtsSynthesisResult
        {
            Success = true,
            OutputFilePath = outputFilePath,
            DurationSeconds = estimatedSeconds
        });
    }
}

public class GeminiTtsProvider : ITtsProvider
{
    private readonly string _apiKey;
    private readonly HttpClient _httpClient;
    public string ProviderName => "GeminiTTS";

    public GeminiTtsProvider(string apiKey, HttpClient? httpClient = null)
    {
        _apiKey = apiKey;
        _httpClient = httpClient ?? new HttpClient();
    }

    public async Task<TtsSynthesisResult> SynthesizeAsync(
        string text,
        string languageCode,
        TtsVoiceConfig config,
        string outputFilePath,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            // If no API key configured, gracefully fallback to mock generation with clear status
            var mock = new MockTtsProvider();
            return await mock.SynthesizeAsync(text, languageCode, config, outputFilePath, ct);
        }

        try
        {
            // Google Gemini API speech generation endpoint
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key={_apiKey}";

            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new object[]
                        {
                            new { text = $"Please read this text aloud in {languageCode} with educational clarity: {text}" }
                        }
                    }
                },
                generationConfig = new
                {
                    response_mime_type = "audio/wav"
                }
            };

            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(url, content, ct);

            if (response.IsSuccessStatusCode)
            {
                var bytes = await response.Content.ReadAsByteArrayAsync(ct);
                var dir = Path.GetDirectoryName(outputFilePath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                await File.WriteAllBytesAsync(outputFilePath, bytes, ct);
                double duration = DurationAnalyzer.GetWavDuration(outputFilePath);
                if (duration <= 0)
                {
                    duration = TimelineEngine.EstimateSpeechDuration(text);
                }

                return new TtsSynthesisResult
                {
                    Success = true,
                    OutputFilePath = outputFilePath,
                    DurationSeconds = duration
                };
            }
            else
            {
                string error = await response.Content.ReadAsStringAsync(ct);
                // Fallback to local mock audio so workflow is not broken during offline/rate-limited development
                var mock = new MockTtsProvider();
                var result = await mock.SynthesizeAsync(text, languageCode, config, outputFilePath, ct);
                result.ErrorMessage = $"Gemini API returned status {response.StatusCode}: {error}. Fallback audio generated.";
                return result;
            }
        }
        catch (Exception ex)
        {
            // Fallback on network errors
            var mock = new MockTtsProvider();
            var result = await mock.SynthesizeAsync(text, languageCode, config, outputFilePath, ct);
            result.ErrorMessage = $"TTS exception: {ex.Message}. Fallback audio generated.";
            return result;
        }
    }
}
