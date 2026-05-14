using Godot;
using Moon.Utils;

namespace Moon.Component;

/// <summary>
/// Record motion of specific canvas item.
/// </summary>
public partial class MotionRecorder2D : Node
{
    [ExportCategory("MotionRecorder2D")]
    [Export]
    public CanvasItem Target { get ;set; }
    
    [Export]
    public bool Disabled { get ;set; } = false;
    
    private Vector2 Velocity = Vector2.Zero;
    public Vector2 GetLastVelocity() => Velocity;
    
    private Vector2 Motion = Vector2.Zero;
    public Vector2 GetLastMotion() => Motion;
    
    private Vector2 LastPosition = Vector2.Zero;
    private bool FirstRecorded = false;

    public MotionRecorder2D() : base()
    {
        Ready += () =>
        {
            this.AddPhysicsProcess(delta =>
            {
                if (Disabled)
                {
                    Velocity = Vector2.Zero;
                    Motion = Vector2.Zero;
                    FirstRecorded = false;
                    return;
                }
                
                var pos = (Vector2)Target.Get(Node2D.PropertyName.GlobalPosition);

                if (!FirstRecorded)
                {
                    FirstRecorded = true;
                    LastPosition = pos;
                    return;
                }
                
                Motion = pos - LastPosition;
                Velocity = Motion / (float)delta;
                LastPosition = pos;
            });
        };
    }
}

public static class MotionRecorder2DExtension
{
    private const string RecorderTag = "_CanvasItemRecorder";

    public static void AddRecorder(this CanvasItem item)
    {
        if (item.HasData(RecorderTag)) return;
        var recorder = new MotionRecorder2D() { Target = item };
        item.AddChildSafely(recorder);
        item.SetData(RecorderTag, recorder);
    }

    public static MotionRecorder2D GetRecorder(this CanvasItem item)
    {
        if (!item.HasData(RecorderTag)) item.AddRecorder();
        return item.GetData<MotionRecorder2D>(RecorderTag);
    }
}