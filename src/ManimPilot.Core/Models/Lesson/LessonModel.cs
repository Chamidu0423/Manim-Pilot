using System.Text.Json.Serialization;

namespace ManimPilot.Core.Models.Lesson;

public class LessonModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("topic")]
    public string Topic { get; set; } = "BinaryToDecimal";

    [JsonPropertyName("language")]
    public string Language { get; set; } = "si-LK";

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("steps")]
    public List<LessonStepModel> Steps { get; set; } = new();

    [JsonPropertyName("settings")]
    public LessonSettings Settings { get; set; } = new();
}

public class LessonStepModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("order_index")]
    public int OrderIndex { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("duration")]
    public double Duration { get; set; } = 3.0;

    [JsonPropertyName("visual")]
    public VisualSpec Visual { get; set; } = new();

    [JsonPropertyName("narration")]
    public NarrationSpec Narration { get; set; } = new();
}

public class VisualSpec
{
    [JsonPropertyName("binary")]
    public string? Binary { get; set; }

    [JsonPropertyName("place_values")]
    public List<long>? PlaceValues { get; set; }

    [JsonPropertyName("active_indices")]
    public List<int>? ActiveIndices { get; set; }

    [JsonPropertyName("expression")]
    public string? Expression { get; set; }

    [JsonPropertyName("result")]
    public string? Result { get; set; }

    [JsonPropertyName("highlight_color")]
    public string? HighlightColor { get; set; } = "#FFD700";

    [JsonPropertyName("custom_props")]
    public Dictionary<string, string> CustomProps { get; set; } = new();
}

public enum TimingRelation
{
    AtEvent,
    AfterEvent,
    BeforeEvent
}

public class NarrationSpec
{
    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;

    [JsonPropertyName("spoken_text")]
    public string? SpokenText { get; set; }

    [JsonPropertyName("anchor")]
    public string? Anchor { get; set; }

    [JsonPropertyName("timing")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TimingRelation Timing { get; set; } = TimingRelation.AfterEvent;

    [JsonPropertyName("offset_seconds")]
    public double OffsetSeconds { get; set; } = 0.2;

    [JsonPropertyName("audio_path")]
    public string? AudioPath { get; set; }

    [JsonPropertyName("audio_duration")]
    public double AudioDuration { get; set; } = 0.0;
}

public class LessonSettings
{
    [JsonPropertyName("width")]
    public int Width { get; set; } = 1920;

    [JsonPropertyName("height")]
    public int Height { get; set; } = 1080;

    [JsonPropertyName("fps")]
    public int Fps { get; set; } = 30;

    [JsonPropertyName("font_name")]
    public string FontName { get; set; } = "Segoe UI";

    [JsonPropertyName("sinhala_font")]
    public string SinhalaFont { get; set; } = "Iskoola Pota";

    [JsonPropertyName("background_color")]
    public string BackgroundColor { get; set; } = "#0D1117";

    [JsonPropertyName("primary_color")]
    public string PrimaryColor { get; set; } = "#58A6FF";

    [JsonPropertyName("accent_color")]
    public string AccentColor { get; set; } = "#7EE787";

    [JsonPropertyName("text_color")]
    public string TextColor { get; set; } = "#F0F6FC";
}
