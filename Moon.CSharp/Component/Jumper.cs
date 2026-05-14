using Godot;
using Godot.Collections;
using Moon.Class;

namespace Moon.Component;

/// <summary>
/// Jump when collides with the ground.
/// </summary>
[GlobalClass, Tool]
public partial class Jumper : Node
{
    [ExportCategory("Jumper")]
    [Export]
    public PhysicsBody2D Platformer { get; set; }
    
    public enum JumpMode { Speed, Height }

    [Export]
    public JumpMode Mode
    {
        get => _mode;
        set
        {
            _mode = value;
            NotifyPropertyListChanged();
        }
    }
    
    private JumpMode _mode = JumpMode.Speed;
    
    [Export]
    public float Speed { get; set; } = 650f;
    
    [Export]
    public float Height { get; set; } = 128f;

    public override void _ValidateProperty(Dictionary property)
    {
        if (
            (Mode == JumpMode.Speed && (string)property["name"] == "Height") ||
            (Mode == JumpMode.Height && (string)property["name"] == "Speed")
        )
        {
            property["usage"] = (uint)PropertyUsageFlags.ReadOnly;
        }
    }

    public Jumper() : base()
    {
    #if TOOLS
        if (Engine.IsEditorHint()) return;            
    #endif
        
        Ready += () =>
        {
            if (Platformer is IPlatformer2D p)
            {
                p.SignalFloorCollided += () => Jump(p);
            }
        };
    }

    public void Jump(IPlatformer2D platformer)
    {
        if (Mode == JumpMode.Speed)
        {
            platformer.SetGravitySpeed(-Speed);
        }
        else
        {
            platformer.Jump(Height);
        }
    }
}