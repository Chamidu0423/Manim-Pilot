using EduMotion.AI;
using EduMotion.Audio;
using EduMotion.Core.Models.Lesson;
using EduMotion.Core.Models.Project;
using EduMotion.Core.Models.Timeline;
using EduMotion.Core.Validation;
using EduMotion.Math;
using EduMotion.Rendering;

namespace EduMotion.App;

public class Program
{
    public static async Task<int> Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        PrintBanner();

        if (args.Length == 0 || args[0] == "--help" || args[0] == "-h")
        {
            PrintHelp();
            Console.WriteLine("\n[Running default demo: Binary 10110110 conversion...]\n");
            return await RunDemoAsync("10110110");
        }

        string command = args[0].ToLowerInvariant();

        switch (command)
        {
            case "demo":
                string sampleBinary = args.Length > 1 ? args[1] : "10110110";
                return await RunDemoAsync(sampleBinary);

            case "new":
                if (args.Length < 2)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Error: Missing binary number argument. Example: edumotion new 10110110");
                    Console.ResetColor();
                    return 1;
                }
                return await CreateNewLessonAsync(args[1], args.Length > 2 ? args[2] : "si-LK");

            case "validate":
                if (args.Length < 2)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Error: Missing project file path. Example: edumotion validate lesson.edumotion");
                    Console.ResetColor();
                    return 1;
                }
                return await ValidateProjectAsync(args[1]);

            case "export":
                if (args.Length < 2)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Error: Missing project file path. Example: edumotion export lesson.edumotion");
                    Console.ResetColor();
                    return 1;
                }
                return await ExportProjectAsync(args[1], args.Length > 2 ? args[2] : "./output");

            default:
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Unknown command '{command}'. Run with --help to see available commands.");
                Console.ResetColor();
                return 1;
        }
    }

    private static void PrintBanner()
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(@"
╔═══════════════════════════════════════════════════════════════════════════╗
║                      EduMotion Studio (ManimPilot)                        ║
║                 AI-Powered Mathematical Animation Creator                 ║
╚═══════════════════════════════════════════════════════════════════════════╝");
        Console.ResetColor();
    }

    private static void PrintHelp()
    {
        Console.WriteLine("Commands:");
        Console.WriteLine("  demo [binary]                  Run full end-to-end pipeline on binary input (default: 10110110)");
        Console.WriteLine("  new <binary> [language]        Generate new lesson, timeline, and .edumotion project");
        Console.WriteLine("  validate <file.edumotion>      Run 5-stage validation (Schema, Math, Timeline, Audio)");
        Console.WriteLine("  export <file.edumotion> [dir]  Export generated Manim scene Python script and audio");
    }

    private static async Task<int> RunDemoAsync(string binaryString)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"[1/5] Calculating deterministic mathematical proof for '{binaryString}'...");
        Console.ResetColor();

        var mathService = new BinaryMathService();
        var mathResult = mathService.ConvertBinaryToDecimal(binaryString);

        if (!mathResult.IsValid)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[Error] Invalid binary: {mathResult.ErrorMessage}");
            Console.ResetColor();
            return 1;
        }

        Console.WriteLine($"  Normalized Binary:  {mathResult.NormalizedBinary}");
        Console.WriteLine($"  Place Values:       {string.Join(", ", mathResult.BitsFromLeftToRight.Select(b => b.PlaceValue))}");
        Console.WriteLine($"  Active Values:      {string.Join(", ", mathResult.ActivePlaceValues)}");
        Console.WriteLine($"  Addition:           {mathResult.AdditionExpression} = {mathResult.DecimalResult}");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"  Verified Result:    {mathResult.DecimalResult} ✓\n");
        Console.ResetColor();

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("[2/5] Synthesizing Lesson Plan & Sinhala Narration...");
        Console.ResetColor();

        var planner = new LessonPlannerService(mathService);
        var lesson = await planner.GenerateBinaryLessonAsync(binaryString, "si-LK");
        Console.WriteLine($"  Lesson Title:       {lesson.Title}");
        Console.WriteLine($"  Language:           {lesson.Language}");
        Console.WriteLine($"  Total Steps:        {lesson.Steps.Count}\n");

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("[3/5] Scheduling Narration & Resolving Timeline Anchors...");
        Console.ResetColor();

        var scheduler = new NarrationScheduler();
        var scheduleResult = scheduler.Schedule(lesson, autoFix: true);

        if (scheduleResult.Adjustments.Count > 0)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine($"  Automatic Schedule Adjustments applied ({scheduleResult.Adjustments.Count}):");
            foreach (var adj in scheduleResult.Adjustments)
            {
                Console.WriteLine($"    - [{adj.StepId}] {adj.Reason}");
            }
            Console.ResetColor();
        }

        var timelineEngine = new TimelineEngine();
        var timeline = timelineEngine.BuildTimeline(lesson);

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"  Timeline constructed: Total Duration = {timeline.TotalDuration:F2} seconds\n");
        Console.ResetColor();

        PrintTimelineView(timeline, lesson);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[4/5] Executing 5-Stage Verification Pipeline...");
        Console.ResetColor();

        var validator = new LessonValidator();
        var lessonReport = validator.Validate(lesson, timeline);

        var mathVerifier = new MathVerifier(mathService);
        var mathReport = mathVerifier.VerifyLessonMath(lesson);

        bool allValid = lessonReport.IsValid && mathReport.IsValid;

        if (allValid)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("  ✓ Schema Validation:   PASSED");
            Console.WriteLine("  ✓ Math Verification:   PASSED (AI result matches calculation)");
            Console.WriteLine("  ✓ Timeline Check:      PASSED");
            Console.WriteLine("  ✓ Audio Overlap Check: PASSED (Zero overlaps detected)\n");
            Console.ResetColor();
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("  ✗ Build Pipeline Rejected with Errors:");
            foreach (var err in lessonReport.Errors.Concat(mathReport.Errors))
            {
                Console.WriteLine($"    - {err.Message}");
            }
            Console.ResetColor();
            return 1;
        }

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("[5/5] Emitting Manim Python Scene & EduMotion Project File...");
        Console.ResetColor();

        string outputDir = Path.Combine(Directory.GetCurrentDirectory(), "output");
        if (!Directory.Exists(outputDir))
        {
            Directory.CreateDirectory(outputDir);
        }

        var generator = new ManimGenerator();
        string pythonScript = generator.GenerateManimScript(lesson, timeline);
        string scriptPath = Path.Combine(outputDir, "scene.py");
        await File.WriteAllTextAsync(scriptPath, pythonScript);

        var project = new EduMotionProject
        {
            Lesson = lesson,
            Timeline = timeline
        };
        project.Metadata.Name = lesson.Title;
        string projectPath = Path.Combine(outputDir, "lesson.edumotion");
        await project.SaveToFileAsync(projectPath);

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"  ✓ Saved Python Scene:  {scriptPath}");
        Console.WriteLine($"  ✓ Saved Project File:  {projectPath}");
        Console.ResetColor();

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("\n==========================================================================");
        Console.WriteLine("Pipeline Execution Completed Successfully! Ready for rendering via Manim.");
        Console.WriteLine("==========================================================================");
        Console.ResetColor();

        return 0;
    }

    private static void PrintTimelineView(TimelineModel timeline, LessonModel lesson)
    {
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("┌────────────────────────────────────────────────────────────────────────┐");
        Console.WriteLine("│ TIMELINE INSPECTOR                                                     │");
        Console.WriteLine("├────────┬─────────────────────────────┬──────────┬──────────────────────┤");
        Console.WriteLine("│ Time   │ Event / Step                │ Track    │ Details              │");
        Console.WriteLine("├────────┼─────────────────────────────┼──────────┼──────────────────────┤");

        var visualTrack = timeline.Tracks.FirstOrDefault(t => t.TrackType == TrackType.Visual);
        var narrationTrack = timeline.Tracks.FirstOrDefault(t => t.TrackType == TrackType.Narration);

        var allEvents = (visualTrack?.Events ?? Enumerable.Empty<TimelineEvent>())
            .Concat(narrationTrack?.Events ?? Enumerable.Empty<TimelineEvent>())
            .OrderBy(e => e.StartTime);

        foreach (var ev in allEvents)
        {
            string timeStr = $"{ev.StartTime:F1}s-{ev.EndTime:F1}s".PadRight(6);
            string titleStr = (ev.Title.Length > 27 ? ev.Title.Substring(0, 24) + "..." : ev.Title).PadRight(27);
            string trackStr = ev.TrackType.ToString().PadRight(8);
            string detailStr = ev.TrackType == TrackType.Narration ? "🔊 Voice narration" : "🎬 Animation visual";

            Console.WriteLine($"│ {timeStr} │ {titleStr} │ {trackStr} │ {detailStr.PadRight(20)} │");
        }

        Console.WriteLine("└────────┴─────────────────────────────┴──────────┴──────────────────────┘");
        Console.ResetColor();
    }

    private static async Task<int> CreateNewLessonAsync(string binary, string language)
    {
        Console.WriteLine($"Creating lesson for binary '{binary}' (language: {language})...");
        return await RunDemoAsync(binary);
    }

    private static async Task<int> ValidateProjectAsync(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error: File not found at '{filePath}'");
            Console.ResetColor();
            return 1;
        }

        var project = await EduMotionProject.LoadFromFileAsync(filePath);
        if (project == null)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Error: Failed to parse project file.");
            Console.ResetColor();
            return 1;
        }

        var validator = new LessonValidator();
        var report = validator.Validate(project.Lesson, project.Timeline);

        var mathVerifier = new MathVerifier();
        var mathReport = mathVerifier.VerifyLessonMath(project.Lesson);

        Console.WriteLine($"Validation Results for '{project.Metadata.Name}':");
        foreach (var issue in report.Issues.Concat(mathReport.Issues))
        {
            Console.WriteLine($"  {issue}");
        }

        bool ok = report.IsValid && mathReport.IsValid;
        Console.ForegroundColor = ok ? ConsoleColor.Green : ConsoleColor.Red;
        Console.WriteLine(ok ? "\nProject is valid!" : "\nProject has errors.");
        Console.ResetColor();

        return ok ? 0 : 1;
    }

    private static async Task<int> ExportProjectAsync(string projectPath, string outputDir)
    {
        if (!File.Exists(projectPath))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error: File not found at '{projectPath}'");
            Console.ResetColor();
            return 1;
        }

        var project = await EduMotionProject.LoadFromFileAsync(projectPath);
        if (project == null) return 1;

        if (!Directory.Exists(outputDir))
        {
            Directory.CreateDirectory(outputDir);
        }

        var generator = new ManimGenerator();
        string script = generator.GenerateManimScript(project.Lesson, project.Timeline);
        string targetScript = Path.Combine(outputDir, "scene.py");
        await File.WriteAllTextAsync(targetScript, script);

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"Successfully exported Manim Python script to: {targetScript}");
        Console.ResetColor();

        return 0;
    }
}
