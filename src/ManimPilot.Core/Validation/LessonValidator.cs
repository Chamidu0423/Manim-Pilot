using ManimPilot.Core.Models.Lesson;
using ManimPilot.Core.Models.Timeline;

namespace ManimPilot.Core.Validation;

public enum ValidationSeverity
{
    Info,
    Warning,
    Error
}

public class ValidationIssue
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? StepId { get; set; }
    public ValidationSeverity Severity { get; set; } = ValidationSeverity.Error;
    public string? SuggestedFix { get; set; }

    public override string ToString() =>
        $"[{Severity}] ({Code}) {(StepId != null ? $"Step: {StepId} - " : "")}{Message}{(SuggestedFix != null ? $" [Fix: {SuggestedFix}]" : "")}";
}

public class ValidationReport
{
    public List<ValidationIssue> Issues { get; } = new();

    public bool IsValid => Issues.All(i => i.Severity != ValidationSeverity.Error);
    public IEnumerable<ValidationIssue> Errors => Issues.Where(i => i.Severity == ValidationSeverity.Error);
    public IEnumerable<ValidationIssue> Warnings => Issues.Where(i => i.Severity == ValidationSeverity.Warning);

    public void AddError(string code, string message, string? stepId = null, string? suggestedFix = null)
    {
        Issues.Add(new ValidationIssue
        {
            Code = code,
            Message = message,
            StepId = stepId,
            Severity = ValidationSeverity.Error,
            SuggestedFix = suggestedFix
        });
    }

    public void AddWarning(string code, string message, string? stepId = null, string? suggestedFix = null)
    {
        Issues.Add(new ValidationIssue
        {
            Code = code,
            Message = message,
            StepId = stepId,
            Severity = ValidationSeverity.Warning,
            SuggestedFix = suggestedFix
        });
    }
}

public interface ILessonValidator
{
    ValidationReport Validate(LessonModel lesson, TimelineModel? timeline = null);
}

public class LessonValidator : ILessonValidator
{
    public ValidationReport Validate(LessonModel lesson, TimelineModel? timeline = null)
    {
        var report = new ValidationReport();

        // 1. Schema / Lesson Basics
        if (string.IsNullOrWhiteSpace(lesson.Title))
        {
            report.AddWarning("L001", "Lesson title is empty.", null, "Provide a descriptive title.");
        }

        if (lesson.Steps == null || lesson.Steps.Count == 0)
        {
            report.AddError("L002", "Lesson contains no steps.", null, "Add at least one educational step.");
            return report;
        }

        var seenIds = new HashSet<string>();
        foreach (var step in lesson.Steps)
        {
            if (string.IsNullOrWhiteSpace(step.Id))
            {
                report.AddError("S001", "Step has missing or empty Id.", null, "Assign unique step Id (e.g. step_01).");
            }
            else if (!seenIds.Add(step.Id))
            {
                report.AddError("S002", $"Duplicate step Id '{step.Id}'.", step.Id, "Ensure all step Ids are globally unique.");
            }

            if (step.Duration <= 0)
            {
                report.AddError("S003", $"Step duration ({step.Duration}s) must be positive.", step.Id, "Set duration to a positive number (e.g. 3.0s).");
            }

            // Narration checks
            if (string.IsNullOrWhiteSpace(step.Narration?.Text))
            {
                report.AddWarning("N001", $"Step '{step.Id}' has no narration text.", step.Id, "Add narration text for educational clarity.");
            }
            else
            {
                // Extremely long narration warning
                if (step.Narration.Text.Length > 600)
                {
                    report.AddWarning("N002", $"Narration in '{step.Id}' is unusually long ({step.Narration.Text.Length} chars).", step.Id, "Split into multiple smaller steps.");
                }

                // Check audio duration vs animation step duration
                if (step.Narration.AudioDuration > 0 && step.Narration.AudioDuration > (step.Duration + 5.0))
                {
                    report.AddWarning(
                        "N003",
                        $"Narration audio duration ({step.Narration.AudioDuration:F1}s) exceeds visual step duration ({step.Duration:F1}s).",
                        step.Id,
                        "Timeline engine will auto-expand step, or increase step duration manually."
                    );
                }
            }
        }

        // 2. Timeline checks (if provided)
        if (timeline != null)
        {
            var narrationTrack = timeline.Tracks.FirstOrDefault(t => t.TrackType == TrackType.Narration);
            if (narrationTrack != null)
            {
                var sortedNarration = narrationTrack.Events.OrderBy(e => e.StartTime).ToList();
                for (int i = 0; i < sortedNarration.Count - 1; i++)
                {
                    var current = sortedNarration[i];
                    var next = sortedNarration[i + 1];

                    // Check for overlap: next starts before current finishes
                    if (next.StartTime < current.EndTime)
                    {
                        double overlap = current.EndTime - next.StartTime;
                        report.AddError(
                            "T001",
                            $"Narration overlap detected between '{current.Title}' and '{next.Title}' ({overlap:F2}s overlap).",
                            current.StepId,
                            $"Delay '{next.Title}' by {overlap:F2}s or shorten preceding narration."
                        );
                    }
                }
            }
        }

        return report;
    }
}
