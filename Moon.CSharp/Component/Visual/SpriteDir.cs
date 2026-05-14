using Godot;
using Moon.Utils;

namespace Moon.Component;

[GlobalClass]
public partial class SpriteDir : Node, IFlipHInit
{
    /// <summary>
    /// The monitored moving node.
    /// </summary>
    [ExportCategory("SpriteDir")]
    [Export]
    public CanvasItem Root { get ;set; }
    
    /// <summary>
    /// Default value is parent.
    /// </summary>
    [Export]
    public CanvasItem Sprite { get ;set; }
    
    [Export]
    public Rotator Rotator { get ;set; }
    
    [Export]
    public bool Flip { get ;set; }
    
    [Export]
    public bool Disabled { get ;set; }

    private MotionRecorder2D Recorder;
    public override void _EnterTree()
    {
        if (Sprite == null && GetParent() is CanvasItem parent) Sprite = parent;
        if (Root != null) Recorder = Root.GetRecorder();
    }

    public override void _Ready()
    {
        this.AddPhysicsProcess(Process);
    }

    public void FlipHInit()
    {
        Connect(
            Node.SignalName.TreeEntered,
            Callable.From(() => SetSpriteFlip(true)),
            (uint)ConnectFlags.OneShot
        );
    }
    
    private void Process()
    {
        if (Root != null)
        {
            var s = Recorder.GetLastMotion().X;
            if (s != 0f) SetSpriteFlip(s < 0f);
        }
    }

    protected void SetSpriteFlip(bool value)
    {
        if (Disabled) return;
        
        var result = Flip ? !value : value;
        Sprite.TrySetFlipH(result);
        Rotator?.Flip = result;
    }
}