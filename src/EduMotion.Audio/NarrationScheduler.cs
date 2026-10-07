using EduMotion.Core.Models.Lesson;
using EduMotion.Core.Models.Timeline;

namespace EduMotion.Audio;

public class ScheduleAdjustment
{
    public string StepId { get; set; } = string.Empty;
    public string StepTitle { get; set; } = string.Empty;
    public double OriginalDuration { get; set; }
    public double AdjustedDuration { get; set; }
    public double OriginalOffset { get; set; }
    public double AdjustedOffset { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class ScheduleResult
{
    public bool HasOverlaps { get; set; }
    public List<string> OverlapErrors { get; set; } = new();
    public List<ScheduleAdjustment> Adjustments { get; set; } = new();
    public double TotalNarrationDuration { get; set; }
    public double TotalLessonDuration { get; set; }
}

public interface INarrationScheduler
{
    ScheduleResult Schedule(LessonModel lesson, bool autoFix = true, double minGapBetweenNarrations = 0.3);
}

public class NarrationScheduler : INarrationScheduler
{
    public ScheduleResult Schedule(LessonModel lesson, bool autoFix = true, double minGapBetweenNarrations = 0.3)
    {
        var result = new ScheduleResult();
        double currentTimelineTime = 0.0;
        double previousNarrationEndTime = 0.0;

        foreach (var step in lesson.Steps.OrderBy(s => s.OrderIndex))
        {
            double stepStart = currentTimelineTime;

            // 1. Determine audio duration
            double audioDuration = step.Narration.AudioDuration;
            if (audioDuration <= 0 && !string.IsNullOrWhiteSpace(step.Narration.Text))
            {
                if (!string.IsNullOrEmpty(step.Narration.AudioPath) && File.Exists(step.Narration.AudioPath))
                {
                    audioDuration = DurationAnalyzer.GetWavDuration(step.Narration.AudioPath);
                }

                if (audioDuration <= 0)
                {
                    audioDuration = TimelineEngine.EstimateSpeechDuration(step.Narration.Text);
                }

                step.Narration.AudioDuration = audioDuration;
            }

            double narrationStart = stepStart + step.Narration.OffsetSeconds;
            double narrationEnd = narrationStart + audioDuration;

            // Check overlap with previous narration
            if (previousNarrationEndTime > 0 && narrationStart < previousNarrationEndTime)
            {
                result.HasOverlaps = true;
                double overlap = previousNarrationEndTime - narrationStart;
                string errorMsg = $"Narration overlap detected in step '{step.Id}' ({step.Title}): Previous narration ends at {previousNarrationEndTime:F2}s, but current starts at {narrationStart:F2}s ({overlap:F2}s overlap).";
                result.OverlapErrors.Add(errorMsg);

                if (autoFix)
                {
                    double neededDelay = (previousNarrationEndTime - narrationStart) + minGapBetweenNarrations;
                    double originalOffset = step.Narration.OffsetSeconds;
                    step.Narration.OffsetSeconds += neededDelay;

                    narrationStart = stepStart + step.Narration.OffsetSeconds;
                    narrationEnd = narrationStart + audioDuration;

                    result.Adjustments.Add(new ScheduleAdjustment
                    {
                        StepId = step.Id,
                        StepTitle = step.Title,
                        OriginalOffset = originalOffset,
                        AdjustedOffset = step.Narration.OffsetSeconds,
                        Reason = $"Resolved narration overlap: shifted narration start by +{neededDelay:F2}s."
                    });
                }
            }

            // Ensure step duration accommodates both visual duration and narration
            double requiredStepDuration = Math.Max(step.Duration, (step.Narration.OffsetSeconds + audioDuration + minGapBetweenNarrations));
            if (requiredStepDuration > step.Duration)
            {
                if (autoFix)
                {
                    result.Adjustments.Add(new ScheduleAdjustment
                    {
                        StepId = step.Id,
                        StepTitle = step.Title,
                        OriginalDuration = step.Duration,
                        AdjustedDuration = requiredStepDuration,
                        Reason = $"Extended step duration to fit full speech ({audioDuration:F2}s) plus padding."
                    });
                    step.Duration = Math.Round(requiredStepDuration, 2);
                }
            }

            if (audioDuration > 0)
            {
                previousNarrationEndTime = narrationEnd;
            }

            currentTimelineTime += step.Duration;
        }

        result.TotalLessonDuration = currentTimelineTime;
        result.TotalNarrationDuration = previousNarrationEndTime;
        return result;
    }
}
