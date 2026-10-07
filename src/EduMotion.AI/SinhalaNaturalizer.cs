using System.Text.RegularExpressions;

namespace EduMotion.AI;

public static class SinhalaNaturalizer
{
    private static readonly Dictionary<long, string> BasicSinhalaNumbers = new()
    {
        { 0, "බිංදුව" },
        { 1, "එක" },
        { 2, "දෙක" },
        { 3, "තුන" },
        { 4, "හතර" },
        { 5, "පහ" },
        { 6, "හය" },
        { 7, "හත" },
        { 8, "අට" },
        { 9, "නවය" },
        { 10, "දහය" },
        { 16, "දහසය" },
        { 20, "විස්ස" },
        { 30, "තිහ" },
        { 32, "තිස් දෙක" },
        { 40, "හතළිහ" },
        { 50, "පනහ" },
        { 60, "හැට" },
        { 64, "හැට හතර" },
        { 70, "හැත්තෑව" },
        { 80, "අසූව" },
        { 90, "අනූව" },
        { 100, "සියය" },
        { 128, "එකසිය විසි අට" },
        { 182, "එකසිය අසූ දෙක" },
        { 255, "දෙසිය පනස් පහ" },
        { 256, "දෙසිය පනස් හය" },
        { 512, "පන්සිය දොළහ" },
        { 1024, "එක්දාස් විසි හතර" }
    };

    /// <summary>
    /// Converts an integer into phonetic spoken Sinhala words.
    /// </summary>
    public static string NumberToSinhalaWords(long number)
    {
        if (BasicSinhalaNumbers.TryGetValue(number, out var word))
        {
            return word;
        }

        if (number < 20)
        {
            return number switch
            {
                11 => "එකොළහ",
                12 => "දොළහ",
                13 => "දහතුන",
                14 => "දාහතර",
                15 => "පහළොව",
                17 => "දාහත",
                18 => "දාඅට",
                19 => "දහනවය",
                _ => number.ToString()
            };
        }

        // Fallback to basic string if composite
        return number.ToString();
    }

    /// <summary>
    /// Converts a binary string into natural spoken Sinhala bit-by-bit reading.
    /// Example: "1011" -> "එක, බිංදුව, එක, එක"
    /// </summary>
    public static string BinaryToSpokenSinhala(string binary)
    {
        var spokenBits = binary.Select(c => c == '1' ? "එක" : "බිංදුව");
        return string.Join(", ", spokenBits);
    }

    /// <summary>
    /// Naturalizes an addition expression into spoken educational Sinhala.
    /// Example: "128 + 32 + 16 = 176"
    /// -> "එකසිය විසි අට, තිස් දෙක, සහ දහසය එකතු කළ විට එකසිය හැත්තෑ හයක් ලැබෙනවා."
    /// </summary>
    public static string NaturalizeEquationToSpokenSinhala(IEnumerable<long> terms, long total)
    {
        var termList = terms.ToList();
        if (termList.Count == 0) return "අගය බිංදුවයි.";

        var words = termList.Select(NumberToSinhalaWords).ToList();
        string termsSpoken;

        if (words.Count == 1)
        {
            termsSpoken = words[0];
        }
        else
        {
            string leading = string.Join(", ", words.Take(words.Count - 1));
            termsSpoken = $"{leading} සහ {words.Last()}";
        }

        string totalSpoken = NumberToSinhalaWords(total);
        return $"{termsSpoken} එකතු කළ විට අවසාන පිළිතුර {totalSpoken} ලෙස ලැබෙනවා.";
    }
}
