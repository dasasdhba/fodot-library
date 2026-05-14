using Godot;
using Godot.Collections;
using Moon.Component;
using Moon.Utils;

namespace Moon.Class;

[GlobalClass, Tool]
public partial class PlatformerMove2D : Node, IFlipHInit
{
    public enum PlatformerMove2DMoveMode { Linear, Accelerate }
    public enum PlatformerMove2DTurnMode { None, Reverse, Clear }

    [ExportCategory("PlatformerMove2D")]
    [Export]
    public PhysicsBody2D Platformer { get ;set; }
    
    [Export]
    public bool Disabled { get; set; } = false;
    
    [Export]
    public PlatformerMove2DTurnMode TurnMode { get;set; } = PlatformerMove2DTurnMode.Reverse;

    [ExportGroup("MovementSetting")]
    [Export]
    public PlatformerMove2DMoveMode MoveMode
    {
        get => _moveMode;
        set
        {
            _moveMode = value;
            NotifyPropertyListChanged();
        }
    }
    private PlatformerMove2DMoveMode _moveMode;
    
    [Export]
    public float Speed { get; set; }
    
    [Export(PropertyHint.Enum, "Left:-1, Idle:0, Right:1")]
    public int Direction { get ;set; }
    
    [Export]
    public float MaxSpeed { get ;set; } = 300f;
    
    [Export]
    public float AccSpeed { get ;set; } = 1000f;
    
    [Export]
    public float DecSpeed { get ;set; } = 1000f;
    
    [Export]
    public float TurnDec { get ;set; } = 1000f;
    
    /// <summary>
    /// not recommend to close, especially for RigidBody
    /// </summary>
    [Export]
    public bool IgnorePhysics { get; set; } = true;

    public void FlipHInit()
    {
        Speed *= -1f;
        if (MoveMode == PlatformerMove2DMoveMode.Accelerate)
        {
            Direction *= -1;
        }
    }
    
    public override void _ValidateProperty(Dictionary property)
    {
        var acc = MoveMode == PlatformerMove2DMoveMode.Accelerate;
        if (
            ((string)property["name"] == "Direction" && !acc) ||
            ((string)property["name"] == "MaxSpeed" && !acc) ||
            ((string)property["name"] == "AccSpeed" && !acc) ||
            ((string)property["name"] == "DecSpeed" && !acc) ||
            ((string)property["name"] == "TurnDec" && !acc) ||
            ((string)property["name"] == "IgnorePhysics" && !acc)
        )
        {
            property["usage"] = (uint)PropertyUsageFlags.ReadOnly;
        }
    }
    
    public PlatformerMove2D() : base()
    {
#if TOOLS
        if (Engine.IsEditorHint()) return;
#endif
        
        Ready += () =>
        {
            if (Platformer is IPlatformer2D platformer)
            {
                platformer.SignalWallCollided += (dir) => Turn(platformer);
                platformer.SetMoveSpeed(Speed, IsUpdatePhysics());
            }
            
            this.AddPhysicsProcess(SetMoveSpeed);
        };
    }
    
    private bool IsUpdatePhysics() => MoveMode == PlatformerMove2DMoveMode.Accelerate && !IgnorePhysics;
    
    public void Turn(IPlatformer2D platformer)
    {
#if TOOLS    
        if (Engine.IsEditorHint()) return;
#endif
    
        if (Disabled || TurnMode == PlatformerMove2DTurnMode.None) return;
        
        if (MoveMode == PlatformerMove2DMoveMode.Accelerate) Direction *= -1;

        if (TurnMode == PlatformerMove2DTurnMode.Clear) Speed = 0f;
        else Speed *= -1f;
        
        platformer.SetMoveSpeed(Speed, IsUpdatePhysics());
    }

    public void SetMoveSpeed(double delta)
    {
#if TOOLS 
        if (Engine.IsEditorHint()) return;
#endif

        if (Disabled) return;
        
        if (Platformer is IPlatformer2D platformer)
        {
            if (MoveMode == PlatformerMove2DMoveMode.Accelerate)
            {
                if (!IgnorePhysics) Speed = platformer.GetLastMoveSpeed();
                
                double speed;
                if (Direction > 0)
                {
                    speed = Speed >= 0f ? Mathe.Accelerate(Speed, AccSpeed, DecSpeed, MaxSpeed, delta) :
                        Mathe.Accelerate(Speed, TurnDec, DecSpeed, 0d, delta);
                }
                else if (Direction < 0)
                {
                    speed = Speed <= 0f ? Mathe.Accelerate(Speed, DecSpeed, AccSpeed, -MaxSpeed, delta) :
                        Mathe.Accelerate(Speed, DecSpeed, TurnDec, 0d, delta);
                }
                else
                {
                    speed = Speed >= 0f ? Mathe.Accelerate(Speed, AccSpeed, DecSpeed, 0d, delta) :
                        Mathe.Accelerate(Speed, DecSpeed, AccSpeed, 0d, delta);
                }
                
                Speed = (float)speed;
            }
            
            platformer.SetMoveSpeed(Speed);
        }
    }
    
    public bool IsMoving() => !IsStopping() && !IsTurning();
    public bool IsStopping() => Direction == 0;

    public bool IsTurning()
    {
        if (Platformer is IPlatformer2D platformer)
        {
            return Direction * platformer.GetLastMoveSpeed() < 0f;
        }
        
        return false;
    }
}