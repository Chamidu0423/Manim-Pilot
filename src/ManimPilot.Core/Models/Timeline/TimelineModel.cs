using System.Text.Json.Serialization;
using ManimPilot.Core.Models.Lesson;

namespace ManimPilot.Core.Models.Timeline;

public enum TrackType
{
    Visual,
    Narration,
    Marker
}

public class TimelineEvent
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    [JsonPropertyName("step_id")]
    public string StepId { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("track_type")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TrackType TrackType { get; set; }

    [JsonPropertyName("start_time")]
    public double StartTime { get; set; }

    [JsonPropertyName("end_time")]
    public double EndTime { get; set; }

    [JsonPropertyName("duration")]
    public double Duration => Math.Max(0.0, EndTime - StartTime);

    [JsonPropertyName("anchor_id")]
    public string? AnchorId { get; set; }

    [JsonPropertyName("payload")]
    public string? Payload { get; set; }
}

public class TimelineTrack
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("track_type")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TrackType TrackType { get; set; }

    [JsonPropertyName("events")]
    public List<TimelineEvent> Events { get; set; } = new();
}

public class TimelineModel
{
    [JsonPropertyName("tracks")]
    public List<TimelineTrack> Tracks { get; set; } = new();

    [JsonPropertyName("total_duration")]
    public double TotalDuration { get; set; }

    public TimelineTrack GetOrCreateTrack(string name, TrackType type)
    {
        var track = Tracks.FirstOrDefault(t => t.Name == name);
        if (track == null)
        {
            track = new TimelineTrack { Name = name, TrackType = type };
            Tracks.Add(track);
        }
        return track;
    }
}

public class TimelineEngine
{
    /// <summary>
    /// Builds a time-aligned timeline from a lesson plan and audio duration metadata.
    /// Ensures visual events and narration are synchronised with proper anchor resolution.
    /// </summary>
    public TimelineModel BuildTimeline(LessonModel lesson)
    {
        var timeline = new TimelineModel();
        var visualTrack = timeline.GetOrCreateTrack("Visual Track", TrackType.Visual);
        var narrationTrack = timeline.GetOrCreateTrack("Narration Track", TrackType.Narration);
        var markerTrack = timeline.GetOrCreateTrack("Markers", TrackType.Marker);

        double currentTime = 0.0;
        const double MIN_STEP_BUFFER = 0.4; // breathing room between steps

        foreach (var step in lesson.Steps.OrderBy(s => s.OrderIndex))
        {
            double stepStartTime = currentTime;

            // Visual duration: minimum duration allocated for visual animations
            double visualDuration = Math.Max(1.0, step.Duration);
            
            // Audio duration: if audio is provided or estimated
            double audioDuration = step.Narration.AudioDuration > 0
                ? step.Narration.AudioDuration
                : EstimateSpeechDuration(step.Narration.Text);

            // Anchor logic:
            // If narration starts with offset, determine its exact start
            double narrationOffset = Math.Max(0.0, step.Narration.OffsetSeconds);
            double narrationStartTime = stepStartTime + narrationOffset;
            double narrationEndTime = narrationStartTime + audioDuration;

            // The step must be at least long enough to cover both visual animation and complete narration
            double totalStepDuration = Math.Max(visualDuration, narrationOffset + audioDuration + MIN_STEP_BUFFER);
            double stepEndTime = stepStartTime + totalStepDuration;

            // 1. Visual Event
            visualTrack.Events.Add(new TimelineEvent
            {
                StepId = step.Id,
                Title = $"{step.Title} (Visual)",
                TrackType = TrackType.Visual,
                StartTime = stepStartTime,
                EndTime = stepStartTime + visualDuration,
                AnchorId = step.Narration.Anchor ?? $"{step.Id}_visual_trigger",
                Payload = step.Type
            });

            // 2. Narration Event
            if (!string.IsNullOrWhiteSpace(step.Narration.Text))
            {
                narrationTrack.Events.Add(new TimelineEvent
                {
                    StepId = step.Id,
                    Title = $"{step.Title} (Speech)",
                    TrackType = TrackType.Narration,
                    StartTime = narrationStartTime,
                    EndTime = narrationEndTime,
                    AnchorId = step.Narration.Anchor ?? $"{step.Id}_visual_trigger",
                    Payload = string.IsNullOrWhiteSpace(step.Narration.SpokenText) ? step.Narration.Text : step.Narration.SpokenText
                });
            }

            // 3. Step Marker
            markerTrack.Events.Add(new TimelineEvent
            {
                StepId = step.Id,
                Title = $"Step: {step.Title}",
                TrackType = TrackType.Marker,
                StartTime = stepStartTime,
                EndTime = stepEndTime,
                Payload = step.Id
            });

            currentTime = stepEndTime;
        }

        timeline.TotalDuration = currentTime;
        return timeline;
    }

    public static double EstimateSpeechDuration(string text, double wordsPerMinute = 130.0)
    {
        if (string.IsNullOrWhiteSpace(text)) return 0.0;
        // Approximation: count words (Sinhala or English)
        var words = text.Split(new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        double duration = (words.Length / wordsPerMinute) * 60.0;
        return Math.Max(1.5, Math.Round(duration, 2));
    }
}
