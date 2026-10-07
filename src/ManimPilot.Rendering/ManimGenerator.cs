using System.Text;
using ManimPilot.Core.Models.Lesson;
using ManimPilot.Core.Models.Timeline;

namespace ManimPilot.Rendering;

public interface IManimGenerator
{
    string GenerateManimScript(LessonModel lesson, TimelineModel? timeline = null);
}

public class ManimGenerator : IManimGenerator
{
    public string GenerateManimScript(LessonModel lesson, TimelineModel? timeline = null)
    {
        var sb = new StringBuilder();

        // 1. Python imports & configuration
        sb.AppendLine("# ==============================================================================");
        sb.AppendLine("# ManimPilot Studio - Automatically Generated Manim Scene Script");
        sb.AppendLine($"# Lesson: {lesson.Title} (ID: {lesson.Id})");
        sb.AppendLine($"# Generated at: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
        sb.AppendLine("# ==============================================================================");
        sb.AppendLine("from manim import *");
        sb.AppendLine("import numpy as np");
        sb.AppendLine();

        // Scene configuration
        sb.AppendLine("config.pixel_width = 1920");
        sb.AppendLine("config.pixel_height = 1080");
        sb.AppendLine("config.frame_rate = 30");
        sb.AppendLine($"config.background_color = \"{lesson.Settings.BackgroundColor}\"");
        sb.AppendLine();

        // Scene class
        sb.AppendLine("class ManimPilotLessonScene(Scene):");
        sb.AppendLine("    def construct(self):");
        sb.AppendLine("        # Global styling colors");
        sb.AppendLine($"        COLOR_PRIMARY = \"{lesson.Settings.PrimaryColor}\"");
        sb.AppendLine($"        COLOR_ACCENT = \"{lesson.Settings.AccentColor}\"");
        sb.AppendLine($"        COLOR_TEXT = \"{lesson.Settings.TextColor}\"");
        sb.AppendLine("        COLOR_MUTED = \"#8B949E\"");
        sb.AppendLine("        COLOR_BG_CARD = \"#161B22\"");
        sb.AppendLine();

        // Title header
        sb.AppendLine("        # Header Title");
        string safeTitle = lesson.Title.Replace("\"", "\\\"");
        sb.AppendLine($"        title = Text(\"{safeTitle}\", font_size=36, color=COLOR_TEXT)");
        sb.AppendLine("        title.to_edge(UP, buff=0.6)");
        sb.AppendLine("        self.play(FadeIn(title, shift=DOWN*0.3), run_time=0.8)");
        sb.AppendLine("        self.wait(0.5)");
        sb.AppendLine();

        // Process steps
        int stepNum = 0;
        foreach (var step in lesson.Steps.OrderBy(s => s.OrderIndex))
        {
            stepNum++;
            sb.AppendLine($"        # ------------------------------------------------------------");
            sb.AppendLine($"        # Step {stepNum:D2}: {step.Title} (Type: {step.Type})");
            sb.AppendLine($"        # ------------------------------------------------------------");

            // Look up timeline event for this step if available
            double stepDuration = step.Duration;
            if (timeline != null)
            {
                var marker = timeline.Tracks.FirstOrDefault(t => t.TrackType == TrackType.Marker)
                    ?.Events.FirstOrDefault(e => e.StepId == step.Id);
                if (marker != null)
                {
                    stepDuration = marker.Duration;
                }
            }

            GenerateStepCode(sb, step, stepDuration);
            sb.AppendLine();
        }

        // Outro / Final hold
        sb.AppendLine("        # Lesson Conclusion");
        sb.AppendLine("        self.wait(1.5)");
        sb.AppendLine("        self.play(*[FadeOut(mob) for mob in self.mobjects], run_time=1.0)");
        sb.AppendLine("        self.wait(0.5)");

        return sb.ToString();
    }

    private void GenerateStepCode(StringBuilder sb, LessonStepModel step, double stepDuration)
    {
        switch (step.Type.ToLowerInvariant())
        {
            case "show_binary":
                GenerateShowBinary(sb, step);
                break;

            case "show_place_values":
                GenerateShowPlaceValues(sb, step);
                break;

            case "select_bits":
            case "bit_selection":
                GenerateSelectBits(sb, step);
                break;

            case "addition_expression":
                GenerateAdditionExpression(sb, step);
                break;

            case "calculate":
            case "final_answer":
                GenerateFinalAnswer(sb, step);
                break;

            default:
                GenerateGenericStep(sb, step);
                break;
        }

        // Add calculated wait time to synchronize animation with audio timeline
        double animRunTime = Math.Min(stepDuration, 1.5);
        double waitRemaining = Math.Max(0.2, stepDuration - animRunTime);
        sb.AppendLine($"        self.wait({waitRemaining:F2})");
    }

    private void GenerateShowBinary(StringBuilder sb, LessonStepModel step)
    {
        string binary = step.Visual.Binary ?? "10110110";
        sb.AppendLine($"        # Visualizing binary bits: {binary}");
        sb.AppendLine("        bits = []");
        sb.AppendLine("        bit_boxes = []");
        sb.AppendLine($"        binary_str = \"{binary}\"");
        sb.AppendLine("        for i, char in enumerate(binary_str):");
        sb.AppendLine("            box = RoundedRectangle(corner_radius=0.15, width=0.9, height=1.1, color=COLOR_PRIMARY, fill_color=COLOR_BG_CARD, fill_opacity=0.8)");
        sb.AppendLine("            label = Text(char, font_size=40, font=\"monospace\", weight=BOLD, color=COLOR_TEXT)");
        sb.AppendLine("            group = VGroup(box, label)");
        sb.AppendLine("            bits.append(group)");
        sb.AppendLine("        self.binary_group = VGroup(*bits).arrange(RIGHT, buff=0.18)");
        sb.AppendLine("        self.binary_group.move_to(UP * 1.2)");
        sb.AppendLine("        self.play(LaggedStart(*[FadeIn(b, shift=DOWN*0.4) for b in self.binary_group], lag_ratio=0.08), run_time=1.0)");
    }

    private void GenerateShowPlaceValues(StringBuilder sb, LessonStepModel step)
    {
        var values = step.Visual.PlaceValues ?? new List<long> { 128, 64, 32, 16, 8, 4, 2, 1 };
        string valuesListStr = $"[{string.Join(", ", values)}]";
        sb.AppendLine($"        # Visualizing place values: {valuesListStr}");
        sb.AppendLine($"        place_values = {valuesListStr}");
        sb.AppendLine("        pv_mobjects = []");
        sb.AppendLine("        for i, val in enumerate(place_values):");
        sb.AppendLine("            pv_text = Text(str(val), font_size=24, color=COLOR_MUTED)");
        sb.AppendLine("            pv_text.next_to(self.binary_group[i], UP, buff=0.25)");
        sb.AppendLine("            pv_mobjects.append(pv_text)");
        sb.AppendLine("        self.place_values_group = VGroup(*pv_mobjects)");
        sb.AppendLine("        self.play(LaggedStart(*[Write(pv) for pv in self.place_values_group], lag_ratio=0.06), run_time=0.9)");
    }

    private void GenerateSelectBits(StringBuilder sb, LessonStepModel step)
    {
        sb.AppendLine("        # Highlighting active bits (bit = 1)");
        sb.AppendLine("        highlight_anims = []");
        sb.AppendLine("        for i, item in enumerate(self.binary_group):");
        sb.AppendLine("            bit_label = item[1].text");
        sb.AppendLine("            if bit_label == \"1\":");
        sb.AppendLine("                highlight_anims.append(item[0].animate.set_stroke(color=COLOR_ACCENT, width=4))");
        sb.AppendLine("                highlight_anims.append(self.place_values_group[i].animate.set_color(COLOR_ACCENT))");
        sb.AppendLine("            else:");
        sb.AppendLine("                highlight_anims.append(item.animate.set_opacity(0.35))");
        sb.AppendLine("                highlight_anims.append(self.place_values_group[i].animate.set_opacity(0.35))");
        sb.AppendLine("        self.play(*highlight_anims, run_time=0.9)");
    }

    private void GenerateAdditionExpression(StringBuilder sb, LessonStepModel step)
    {
        string expr = step.Visual.Expression ?? "128 + 32 + 16 + 4 + 2";
        string safeExpr = expr.Replace("\"", "\\\"");
        sb.AppendLine($"        # Displaying sum equation: {safeExpr}");
        sb.AppendLine($"        self.expr_text = MathTex(\"{safeExpr}\", font_size=42, color=COLOR_TEXT)");
        sb.AppendLine("        self.expr_text.move_to(DOWN * 0.8)");
        sb.AppendLine("        self.play(Write(self.expr_text), run_time=1.1)");
    }

    private void GenerateFinalAnswer(StringBuilder sb, LessonStepModel step)
    {
        string result = step.Visual.Result ?? "182";
        string expr = step.Visual.Expression ?? "128 + 32 + 16 + 4 + 2";
        string fullEq = $"{expr} = {result}";
        sb.AppendLine($"        # Transforming equation into final answer: {fullEq}");
        sb.AppendLine($"        full_eq = MathTex(\"{fullEq}\", font_size=46, color=COLOR_ACCENT)");
        sb.AppendLine("        full_eq.move_to(DOWN * 0.8)");
        sb.AppendLine("        box = SurroundingRectangle(full_eq, buff=0.25, color=COLOR_ACCENT, corner_radius=0.1)");
        sb.AppendLine("        if hasattr(self, 'expr_text'):");
        sb.AppendLine("            self.play(ReplacementTransform(self.expr_text, full_eq), Create(box), run_time=1.0)");
        sb.AppendLine("        else:");
        sb.AppendLine("            self.play(FadeIn(full_eq), Create(box), run_time=1.0)");
        sb.AppendLine("        self.play(Circumscribe(box, color=COLOR_ACCENT, time_width=0.4), run_time=0.8)");
    }

    private void GenerateGenericStep(StringBuilder sb, LessonStepModel step)
    {
        string desc = string.IsNullOrEmpty(step.Visual.Expression) ? step.Title : step.Visual.Expression;
        string safeDesc = desc.Replace("\"", "\\\"");
        sb.AppendLine($"        # Generic step display: {safeDesc}");
        sb.AppendLine($"        generic_text = Text(\"{safeDesc}\", font_size=32, color=COLOR_TEXT)");
        sb.AppendLine("        generic_text.move_to(DOWN * 0.5)");
        sb.AppendLine("        self.play(FadeIn(generic_text), run_time=0.8)");
    }
}
