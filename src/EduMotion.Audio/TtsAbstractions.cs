using System.Text.Json.Serialization;

namespace EduMotion.Audio;

public class TtsVoiceConfig
{
    public string VoiceName { get; set; } = "default";
    public string Gender { get; set; } = "female"; // female / male
    public double SpeakingRate { get; set; } = 1.0;
    public double Pitch { get; set; } = 1.0;
}

public class TtsSynthesisResult
{
    public bool Success { get; set; }
    public string OutputFilePath { get; set; } = string.Empty;
    public double DurationSeconds { get; set; }
    public string? ErrorMessage { get; set; }
}

public interface ITtsProvider
{
    string ProviderName { get; }
    Task<TtsSynthesisResult> SynthesizeAsync(
        string text,
        string languageCode,
        TtsVoiceConfig config,
        string outputFilePath,
        CancellationToken ct = default
    );
}
