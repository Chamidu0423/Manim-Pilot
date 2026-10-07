using System.Text.Json.Serialization;

namespace EduMotion.Core.Models.Animation;

public enum AnimationActionType
{
    Show,
    Hide,
    FadeIn,
    FadeOut,
    Transform,
    ReplacementTransform,
    Highlight,
    Indicate,
    Write,
    Wait,
    MoveTo
}

public class AnimationInstruction
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    [JsonPropertyName("action")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public AnimationActionType Action { get; set; }

    [JsonPropertyName("target_id")]
    public string TargetId { get; set; } = string.Empty;

    [JsonPropertyName("source_id")]
    public string? SourceId { get; set; }

    [JsonPropertyName("content")]
    public string? Content { get; set; }

    [JsonPropertyName("is_latex")]
    public bool IsLatex { get; set; }

    [JsonPropertyName("position_x")]
    public double PositionX { get; set; } = 0.0;

    [JsonPropertyName("position_y")]
    public double PositionY { get; set; } = 0.0;

    [JsonPropertyName("scale")]
    public double Scale { get; set; } = 1.0;

    [JsonPropertyName("color")]
    public string? Color { get; set; }

    [JsonPropertyName("duration")]
    public double Duration { get; set; } = 1.0;

    [JsonPropertyName("anchor_id")]
    public string? AnchorId { get; set; }

    [JsonPropertyName("metadata")]
    public Dictionary<string, string> Metadata { get; set; } = new();
}

public class AnimationSceneScript
{
    [JsonPropertyName("scene_name")]
    public string SceneName { get; set; } = "EduMotionScene";

    [JsonPropertyName("instructions")]
    public List<AnimationInstruction> Instructions { get; set; } = new();

    [JsonPropertyName("declared_objects")]
    public HashSet<string> DeclaredObjects { get; set; } = new();
}

public class VisualObjectState
{
    public string ObjectId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public bool IsLatex { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public double Scale { get; set; } = 1.0;
    public string? Color { get; set; }
    public bool IsVisible { get; set; }
}

public class SceneState
{
    public int StepIndex { get; set; }
    public Dictionary<string, VisualObjectState> Objects { get; set; } = new();

    public SceneState Clone()
    {
        var copy = new SceneState { StepIndex = StepIndex };
        foreach (var kvp in Objects)
        {
            copy.Objects[kvp.Key] = new VisualObjectState
            {
                ObjectId = kvp.Value.ObjectId,
                Content = kvp.Value.Content,
                IsLatex = kvp.Value.IsLatex,
                X = kvp.Value.X,
                Y = kvp.Value.Y,
                Scale = kvp.Value.Scale,
                Color = kvp.Value.Color,
                IsVisible = kvp.Value.IsVisible
            };
        }
        return copy;
    }
}

public class SceneStateMachine
{
    private SceneState _currentState = new();

    public SceneState CurrentState => _currentState;

    public void Reset()
    {
        _currentState = new SceneState();
    }

    /// <summary>
    /// Generates the minimal transition instructions needed to reach target state from current state.
    /// Handles: REMOVE obsolete objects, KEEP persistent, UPDATE changed, ADD new.
    /// </summary>
    public List<AnimationInstruction> TransitionTo(SceneState targetState, double transitionDuration = 0.5)
    {
        var instructions = new List<AnimationInstruction>();

        // 1. Remove obsolete objects that were visible in current state but absent or marked invisible in target
        foreach (var curObj in _currentState.Objects.Values.Where(o => o.IsVisible))
        {
            if (!targetState.Objects.TryGetValue(curObj.ObjectId, out var nextObj) || !nextObj.IsVisible)
            {
                instructions.Add(new AnimationInstruction
                {
                    Action = AnimationActionType.FadeOut,
                    TargetId = curObj.ObjectId,
                    Duration = transitionDuration
                });
            }
        }

        // 2. Add or update objects
        foreach (var nextObj in targetState.Objects.Values.Where(o => o.IsVisible))
        {
            if (!_currentState.Objects.TryGetValue(nextObj.ObjectId, out var curObj) || !curObj.IsVisible)
            {
                // New object to display
                instructions.Add(new AnimationInstruction
                {
                    Action = AnimationActionType.FadeIn,
                    TargetId = nextObj.ObjectId,
                    Content = nextObj.Content,
                    IsLatex = nextObj.IsLatex,
                    PositionX = nextObj.X,
                    PositionY = nextObj.Y,
                    Scale = nextObj.Scale,
                    Color = nextObj.Color,
                    Duration = transitionDuration
                });
            }
            else if (curObj.Content != nextObj.Content || Math.Abs(curObj.X - nextObj.X) > 0.001 || Math.Abs(curObj.Y - nextObj.Y) > 0.001)
            {
                // Transform existing object
                instructions.Add(new AnimationInstruction
                {
                    Action = AnimationActionType.Transform,
                    TargetId = nextObj.ObjectId,
                    SourceId = curObj.ObjectId,
                    Content = nextObj.Content,
                    IsLatex = nextObj.IsLatex,
                    PositionX = nextObj.X,
                    PositionY = nextObj.Y,
                    Duration = transitionDuration
                });
            }
        }

        _currentState = targetState.Clone();
        return instructions;
    }
}
