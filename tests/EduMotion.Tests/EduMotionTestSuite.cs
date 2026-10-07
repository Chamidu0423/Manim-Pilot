using EduMotion.AI;
using EduMotion.Audio;
using EduMotion.Core.Models.Lesson;
using EduMotion.Core.Models.Project;
using EduMotion.Core.Models.Timeline;
using EduMotion.Core.Validation;
using EduMotion.Math;
using EduMotion.Rendering;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EduMotion.Tests;

[TestClass]
public class EduMotionTestSuite
{
    [TestMethod]
    public void BinaryMath_ConvertsStandardBinary_Accurately()
    {
        var service = new BinaryMathService();
        var result = service.ConvertBinaryToDecimal("10110110");

        Assert.IsTrue(result.IsValid);
        Assert.AreEqual(182L, result.DecimalResult);
        Assert.AreEqual("128 + 32 + 16 + 4 + 2", result.AdditionExpression);
        Assert.AreEqual(8, result.BitsFromLeftToRight.Count);
        CollectionAssert.AreEqual(new long[] { 128, 32, 16, 4, 2 }, result.ActivePlaceValues);
    }

    [TestMethod]
    public void BinaryMath_HandlesEdgeCases_Correctly()
    {
        var service = new BinaryMathService();

        // 1. All zeros
        var zero = service.ConvertBinaryToDecimal("0000");
        Assert.IsTrue(zero.IsValid);
        Assert.AreEqual(0L, zero.DecimalResult);

        // 2. All ones (8-bit)
        var maxByte = service.ConvertBinaryToDecimal("11111111");
        Assert.IsTrue(maxByte.IsValid);
        Assert.AreEqual(255L, maxByte.DecimalResult);

        // 3. Leading zeros
        var leadingZeros = service.ConvertBinaryToDecimal("00001010");
        Assert.IsTrue(leadingZeros.IsValid);
        Assert.AreEqual(10L, leadingZeros.DecimalResult);

        // 4. Invalid characters
        var invalid = service.ConvertBinaryToDecimal("10201");
        Assert.IsFalse(invalid.IsValid);
        Assert.IsNotNull(invalid.ErrorMessage);

        // 5. Empty string
        var empty = service.ConvertBinaryToDecimal("");
        Assert.IsFalse(empty.IsValid);
    }

    [TestMethod]
    public void MathVerifier_CatchesAiHallucinations()
    {
        var mathService = new BinaryMathService();
        var verifier = new MathVerifier(mathService);

        var lesson = new LessonModel
        {
            Title = "Test Lesson",
            Steps = new List<LessonStepModel>
            {
                new()
                {
                    Id = "step_01",
                    OrderIndex = 1,
                    Type = "show_binary",
                    Duration = 2.0,
                    Visual = new VisualSpec { Binary = "10110110" }
                },
                new()
                {
                    Id = "step_05",
                    OrderIndex = 2,
                    Type = "final_answer",
                    Duration = 2.0,
                    Visual = new VisualSpec
                    {
                        Binary = "10110110",
                        Result = "181" // Hallucinated wrong value! True answer is 182
                    }
                }
            }
        };

        var report = verifier.VerifyLessonMath(lesson);

        Assert.IsFalse(report.IsValid);
        Assert.IsTrue(report.Errors.Any(e => e.Code == "M003"));
    }

    [TestMethod]
    public void NarrationScheduler_DetectsAndFixes_Overlaps()
    {
        var scheduler = new NarrationScheduler();

        var lesson = new LessonModel
        {
            Steps = new List<LessonStepModel>
            {
                new()
                {
                    Id = "s1",
                    OrderIndex = 1,
                    Duration = 2.0,
                    Narration = new NarrationSpec
                    {
                        Text = "First step",
                        OffsetSeconds = 0.0,
                        AudioDuration = 3.5 // Audio longer than step duration
                    }
                },
                new()
                {
                    Id = "s2",
                    OrderIndex = 2,
                    Duration = 2.0,
                    Narration = new NarrationSpec
                    {
                        Text = "Second step",
                        OffsetSeconds = 0.0,
                        AudioDuration = 2.0
                    }
                }
            }
        };

        // Run scheduler with autoFix = true
        var result = scheduler.Schedule(lesson, autoFix: true);

        // Step 1 should have been extended to fit 3.5s + padding
        Assert.IsTrue(lesson.Steps[0].Duration >= 3.8);

        // Step 2 should not overlap step 1
        var timelineEngine = new TimelineEngine();
        var timeline = timelineEngine.BuildTimeline(lesson);

        var validator = new LessonValidator();
        var report = validator.Validate(lesson, timeline);

        Assert.IsTrue(report.IsValid);
        Assert.AreEqual(0, report.Errors.Count(e => e.Code == "T001"));
    }

    [TestMethod]
    public void SinhalaNaturalizer_GeneratesPhoneticSpeech()
    {
        string binarySpoken = SinhalaNaturalizer.BinaryToSpokenSinhala("101");
        Assert.AreEqual("එක, බිංදුව, එක", binarySpoken);

        string eqSpoken = SinhalaNaturalizer.NaturalizeEquationToSpokenSinhala(new long[] { 10, 20 }, 30);
        Assert.IsTrue(eqSpoken.Contains("දහය සහ විස්ස එකතු කළ විට"));
    }

    [TestMethod]
    public void ManimGenerator_EmitsValidSceneScript()
    {
        var generator = new ManimGenerator();
        var lesson = new LessonModel
        {
            Title = "Unit Test Scene",
            Steps = new List<LessonStepModel>
            {
                new()
                {
                    Id = "s1",
                    Title = "Show Bits",
                    Type = "show_binary",
                    Duration = 3.0,
                    Visual = new VisualSpec { Binary = "1101" }
                }
            }
        };

        string pythonScript = generator.GenerateManimScript(lesson);

        Assert.IsTrue(pythonScript.Contains("from manim import *"));
        Assert.IsTrue(pythonScript.Contains("class EduMotionLessonScene(Scene):"));
        Assert.IsTrue(pythonScript.Contains("binary_str = \"1101\""));
        Assert.IsTrue(pythonScript.Contains("self.wait"));
    }

    [TestMethod]
    public void ProjectPersistence_SerializesAndDeserializes_Faithfully()
    {
        var project = new EduMotionProject
        {
            Metadata = new ProjectMetadata { Name = "Binary 101 Test", Author = "Chamidu" },
            Lesson = new LessonModel
            {
                Title = "Binary 101",
                Language = "si-LK"
            }
        };

        string json = project.ToJson();
        var loaded = EduMotionProject.FromJson(json);

        Assert.IsNotNull(loaded);
        Assert.AreEqual("Binary 101 Test", loaded.Metadata.Name);
        Assert.AreEqual("Chamidu", loaded.Metadata.Author);
        Assert.AreEqual("si-LK", loaded.Lesson.Language);
    }
}
