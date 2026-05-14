using System;
using Godot;
using Moon.Utils;

namespace Moon.Class;

// CharacterBody is more suitable for player and other non solid objects.
// Since it's much easier to manipulate compared with RigidBody2D.
// But the physics simulation is not stable enough, especially for stacked boxes.

[GlobalClass]
public partial class CharaPlatformer2D : CharacterBody2D, IPlatformer2D
{
    [ExportCategory("CharaPlatformer2D")]
    [ExportGroup("ProcessSetting")]
    [Export]
    public int CustomFps { get ;set; } = -1;
    
    [Export]
    public bool AutoProcess { get; set; } = true;

    [Export]
    public bool EnableGravity { get; set; } = true;

    [Export]
    public bool EnableMove { get; set; } = true;

    [ExportGroup("Gravity", "Gravity")]
    [Export]
    public float GravityMaxSpeed { get; set; } = 500f;

    [Export]
    public float GravityAccSpeed { get; set; } = 1000f;

    [Export]
    public float GravityDecSpeed { get; set; } = 1000f;
    
    [Export]
    public float GravityWaterMaxSpeed { get; set; } = 150f;

    [Export]
    public float GravityWaterAccSpeed { get; set; } = 250f;

    [Export]
    public float GravityWaterDecSpeed { get; set; } = 2125f;
    
    [Export] 
    public float GravityFloatingHeight { get; set; } = -1f;
    
    [Export]
    public float GravityFloatingSpeed { get; set; } = 50f;
    
    [Export]
    public float GravityFloatingDamp { get; set; } = 4f;

    [ExportGroup("Slope", "Slope")]
    [Export]
    public float SlopeFixLength { get; set; } = 8f;

    [Export]
    public float SlopeFixSafeMargin { get; set; } = 4f;

    [ExportGroup("Collision")]
    [Export(PropertyHint.Layers2DPhysics)]
    public uint WaterMask
    {
        get => _WaterMask;
        set
        {
            _WaterMask = value;
            if (WaterOverlap != null)
                WaterOverlap.CollisionMask = value;
        }
    }
    
    private uint _WaterMask = 1;

    [Signal]
    public delegate void FloorCollidedEventHandler();

    [Signal]
    public delegate void CeilingCollidedEventHandler();

    [Signal]
    public delegate void WallCollidedEventHandler(int dir);

    [Signal]
    public delegate void WaterEnteredEventHandler();

    [Signal]
    public delegate void WaterExitedEventHandler();

    /// <summary>
    /// This is a movement param, NOT the real move speed
    /// Using GetLastMoveSpeed() to get the real speed
    /// </summary>
    public float MoveSpeed { get; set; } = 0f;
    
    public float Gravity { get; set; } = 0f;

    protected OverlapSync2D WaterOverlap { get ;set; }

    public CharaPlatformer2D() : base()
    {
        SlideOnCeiling = false; // this is stupid
        
        Ready += () =>
        {
            WaterOverlap = OverlapSync2D.CreateFrom(this);
            WaterOverlap.CollisionMask = WaterMask;
        
            this.AddPhysicsProcess(delta =>
            {
                if (AutoProcess)
                {
                    PlatformerProcess(CustomFps > 0 ? 1f / CustomFps : delta);
                }
            });
        };
    }

    public Vector2 GetGravityDirection() => -UpDirection;
    public void SetGravitySpeed(float speed) => Gravity = speed;
    public float GetGravitySpeed() => Gravity;
    public void Jump(float height)
    {
        // v^2 = 2ax
        Gravity = -(float)Math.Sqrt(2f * GravityAccSpeed * height);
    }
    
    public Vector2 GetMoveDirection() => -UpDirection.Orthogonal();

    public void SetMoveSpeed(float speed, bool updatePhysics = false)
    {
        MoveSpeed = speed;
        if (updatePhysics) RealMoveSpeed = MoveSpeed;
    }
    public float GetMoveSpeed() => MoveSpeed;

    private float RealMoveSpeed = 0f;
    public float GetLastMoveSpeed() => RealMoveSpeed;
    public float GetLastGravitySpeed() => Gravity;

    private bool InWater = false;
    private bool InWaterFirst = false;

    public bool IsInWater(Vector2 offset)
    {
        return WaterOverlap.IsOverlapping(
            result => result.GetData("Water", false),
            offset,
            true);
    }
    
    public bool IsInWater(bool forceUpdate = false)
    {
        if (!forceUpdate) return InWater;

        InWater = IsInWater(Vector2.Zero);
        return InWater;
    }

    private bool OnWall = false;

    /// <summary>
    /// Alternate of IsOnWall(),
    /// since we have made getting through single pit build-in.
    /// </summary>
    public bool IsReallyOnWall() => OnWall;

    // binary search
    private const float FloatingDetectStep = 256f;
    public float GetFloatingHeight()
    {
        var dir = -GetGravityDirection();
        return Mathe.BinarySearch(
            x => !IsInWater(x * dir),
            FloatingDetectStep
        );
    }

    public void PlatformerProcess(double delta)
    {
        var onFloorLast = IsOnFloor();
        var onCeilingLast = IsOnCeiling();
        var onWallLast = OnWall;
        var inWaterLast = InWater;

        OnWall = false;

        if (EnableGravity)
        {
            // gravity
            
            var waterAcc = InWater && GravityFloatingHeight < 0f;
            
            var gMax = waterAcc ? GravityWaterMaxSpeed : GravityMaxSpeed;
            var gAcc = waterAcc ? GravityWaterAccSpeed : GravityAccSpeed;
            var gDec = waterAcc ? GravityWaterDecSpeed : GravityDecSpeed;
            Gravity = (float)Mathe.Accelerate(Gravity, gAcc, gDec, gMax, delta);
            
            // floating

            if (InWater && GravityFloatingHeight >= 0f)
            {
                var d = GetFloatingHeight() - GravityFloatingHeight;
                Gravity -= (float)((Gravity * GravityFloatingDamp + d * GravityFloatingSpeed) * delta);
            }
        }

        // movement
        
        Velocity = new Vector2(0f, 0f);
        var moveSpeed = 0f;
        var moveDir = GetMoveDirection();
        
        if (EnableGravity) { Velocity -= UpDirection * Gravity; }
        if (EnableMove)
        {
            moveSpeed = MoveSpeed;
            Velocity += moveDir * moveSpeed;
        }

        if (MoveAndSlide(delta))
        {
            if (EnableGravity) Gravity = Velocity.Dot(-UpDirection);
            if (EnableMove)
            {
                RealMoveSpeed = Velocity.Dot(moveDir);
                OnWall = IsOnWall();
            }
        }
        else
        {
            if (EnableMove) RealMoveSpeed = moveSpeed;
        }
        
        // single pit get through

        if (IsOnWall() && SlopeFixLength > 0f
            && EnableGravity && EnableMove && 
            Gravity >= 0f && !Mathf.IsZeroApprox(moveSpeed))
        {
            var testTransform = GlobalTransform;
            testTransform.Origin += SlopeFixLength * UpDirection;
            var testMotion = SlopeFixSafeMargin * Math.Sign(moveSpeed) * moveDir;
            if (!TestMove(testTransform, testMotion))
            {
                RealMoveSpeed = moveSpeed;
                OnWall = false;
                if (!onFloorLast)
                {
                    EmitSignal(SignalName.FloorCollided);
                    onFloorLast = true;
                }

                GlobalPosition += (float)(moveSpeed * delta) * moveDir;
                GlobalPosition += SlopeFixLength * UpDirection;
                MoveAndCollide((SlopeFixLength + 1f) * -UpDirection);
            }
        }

        // signal
        
        if (!onFloorLast && IsOnFloor()) EmitSignal(SignalName.FloorCollided);
        if (!onCeilingLast && IsOnCeiling()) EmitSignal(SignalName.CeilingCollided);
        if (!onWallLast && OnWall) EmitSignal(SignalName.WallCollided, Math.Sign(moveSpeed));
        
        // water update
        
        IsInWater(true);
        if (!InWaterFirst)
        {
            InWaterFirst = true;
            return;
        }
        
        if (!inWaterLast && InWater) EmitSignal(SignalName.WaterEntered);
        if (inWaterLast && !InWater) EmitSignal(SignalName.WaterExited);
    }
}