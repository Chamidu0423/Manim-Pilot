using EduMotion.Core.Models.Lesson;
using EduMotion.Core.Validation;

namespace EduMotion.Math;

public interface IMathVerifier
{
    ValidationReport VerifyLessonMath(LessonModel lesson);
}

public class MathVerifier : IMathVerifier
{
    private readonly IBinaryMathService _binaryMath;

    public MathVerifier(IBinaryMathService? binaryMath = null)
    {
        _binaryMath = binaryMath ?? new BinaryMathService();
    }

    public ValidationReport VerifyLessonMath(LessonModel lesson)
    {
        var report = new ValidationReport();

        // 1. Identify primary binary value if topic is Binary
        string? binaryString = null;
        var firstBinaryStep = lesson.Steps.FirstOrDefault(s => !string.IsNullOrEmpty(s.Visual?.Binary));
        if (firstBinaryStep != null)
        {
            binaryString = firstBinaryStep.Visual?.Binary;
        }

        if (binaryString != null)
        {
            var mathResult = _binaryMath.ConvertBinaryToDecimal(binaryString);
            if (!mathResult.IsValid)
            {
                report.AddError(
                    "M001",
                    $"Invalid binary input in lesson: {mathResult.ErrorMessage}",
                    firstBinaryStep?.Id,
                    "Provide a valid binary sequence consisting solely of 0 and 1."
                );
                return report;
            }

            // Verify steps referencing calculation or result
            foreach (var step in lesson.Steps)
            {
                // Check Place Values step if specified
                if (step.Visual.PlaceValues != null && step.Visual.PlaceValues.Count > 0)
                {
                    var expectedPlaceValues = mathResult.BitsFromLeftToRight.Select(b => b.PlaceValue).ToList();
                    if (!step.Visual.PlaceValues.SequenceEqual(expectedPlaceValues))
                    {
                        report.AddError(
                            "M002",
                            $"Place values mismatch in step '{step.Id}'. Expected [{string.Join(", ", expectedPlaceValues)}], but AI generated [{string.Join(", ", step.Visual.PlaceValues)}].",
                            step.Id,
                            $"Correct place values to: {string.Join(", ", expectedPlaceValues)}"
                        );
                    }
                }

                // Check final result if specified
                if (!string.IsNullOrEmpty(step.Visual.Result))
                {
                    if (long.TryParse(step.Visual.Result.Trim(), out long reportedResult))
                    {
                        if (reportedResult != mathResult.DecimalResult)
                        {
                            report.AddError(
                                "M003",
                                $"Mathematical result mismatch in step '{step.Id}'! AI generated '{reportedResult}', but verified math calculates '{mathResult.DecimalResult}'.",
                                step.Id,
                                $"Correct final result to '{mathResult.DecimalResult}'."
                            );
                        }
                    }
                }

                // Check addition expression if specified
                if (!string.IsNullOrEmpty(step.Visual.Expression))
                {
                    // Check if expression matches active place values
                    string normalizedExpr = step.Visual.Expression.Replace(" ", "");
                    string expectedExpr = mathResult.AdditionExpression.Replace(" ", "");
                    if (step.Type.Contains("add", StringComparison.OrdinalIgnoreCase) && normalizedExpr != expectedExpr)
                    {
                        report.AddWarning(
                            "M004",
                            $"Addition expression in step '{step.Id}' ('{step.Visual.Expression}') differs from canonical expression ('{mathResult.AdditionExpression}').",
                            step.Id,
                            $"Use canonical expression: {mathResult.AdditionExpression}"
                        );
                    }
                }
            }
        }

        return report;
    }
}
