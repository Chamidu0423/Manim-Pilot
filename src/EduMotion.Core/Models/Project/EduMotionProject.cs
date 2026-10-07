using System.Text.Json;
using System.Text.Json.Serialization;
using EduMotion.Core.Models.Lesson;
using EduMotion.Core.Models.Timeline;

namespace EduMotion.Core.Models.Project;

public class ProjectMetadata
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "Untitled Lesson";

    [JsonPropertyName("author")]
    public string Author { get; set; } = "EduMotion User";

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("tags")]
    public List<string> Tags { get; set; } = new();
}

public class EduMotionProject
{
    [JsonPropertyName("version")]
    public int Version { get; set; } = 1;

    [JsonPropertyName("metadata")]
    public ProjectMetadata Metadata { get; set; } = new();

    [JsonPropertyName("lesson")]
    public LessonModel Lesson { get; set; } = new();

    [JsonPropertyName("timeline")]
    public TimelineModel? Timeline { get; set; }

    [JsonPropertyName("build_settings")]
    public Dictionary<string, string> BuildSettings { get; set; } = new();

    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public string ToJson() => JsonSerializer.Serialize(this, JsonOptions);

    public static EduMotionProject? FromJson(string json) =>
        JsonSerializer.Deserialize<EduMotionProject>(json, JsonOptions);

    public static async Task<EduMotionProject?> LoadFromFileAsync(string filePath, CancellationToken ct = default)
    {
        var json = await File.ReadAllTextAsync(filePath, ct);
        return FromJson(json);
    }

    public async Task SaveToFileAsync(string filePath, CancellationToken ct = default)
    {
        Metadata.UpdatedAt = DateTime.UtcNow;
        var dir = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
        var json = ToJson();
        await File.WriteAllTextAsync(filePath, json, ct);
    }
}
