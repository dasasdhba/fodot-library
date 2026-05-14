using System;
using Godot;
using Moon.Utils;

namespace Moon.Class;

[GlobalClass]
public partial class CharaPlatformer3D : CharacterBody3D
{
    [ExportCategory("CharaPlatformer3D")]
    [ExportGroup("ProcessSetting")]
    [Export]
    public int CustomFps { get ;set; } = -1;
    
    [Export]
    public bool AutoProcess { get; set; } = true;

    [Export]
    public bool EnableGravity { get; set; } = true;

    [Export]
    public bool EnableMove { get; set; } = true;
    
    [ExportGroup("Speeds")]
    [Export]
    public float Gravity { get; set; } = 0f;
    
    /// <summary>
    /// Movement param, not the real move speed
    /// </summary>
    [Export]
    public float MoveSpeed { get; set; } = 0f;
    
    [ExportGroup("Gravity", "Gravity")]
    [Export]
    public float GravityMaxSpeed { get; set; } = 15f;

    [Export]
    public float GravityAccSpeed { get; set; } = 40f;

    [Export]
    public float GravityDecSpeed { get; set; } = 40f;
    
    [Export]
    public float GravityWaterMaxSpeed { get; set; } = 3f;

    [Export]
    public float GravityWaterAccSpeed { get; set; } = 10f;

    [Export]
    public float GravityWaterDecSpeed { get; set; } = 60f;
    
    [Export] 
    public float GravityFloatingHeight { get; set; } = -1f;
    
    [Export]
    public float GravityFloatingSpeed { get; set; } = 2f;
    
    [Export]
    public float GravityFloatingDamp { get; set; } = 4f;
    
    [ExportGroup("Move", "Move")]
    [Export]
    public Vector2 MoveDirection { get; set; }

    [ExportGroup("Slope", "Slope")]
    [Export]
    public float SlopeFixLength { get; set; } = 0.05f;

    [Export]
    public float SlopeFixSafeMargin { get; set; } = 0.01f;

    [ExportGroup("Collision")]
    [Export(PropertyHint.Layers3DPhysics)]
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
    public delegate void WallCollidedEventHandler(Vector2 dir);

    [Signal]
    public delegate void WaterEnteredEventHandler();

    [Signal]
    public delegate void WaterExitedEventHandler();

    protected OverlapSync3D WaterOverlap { get ;set; }

    public CharaPlatformer3D() : base()
    {
        SlideOnCeiling = false; // this is stupid
        
        Ready += () =>
        {
            WaterOverlap = OverlapSync3D.CreateFrom(this);
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

    public Vector3 GetGravityDirection() => -UpDirection;
    public void SetGravitySpeed(float speed) => Gravity = speed;
    public float GetGravitySpeed() => Gravity;
    public void Jump(float height)
    {
        // v^2 = 2ax
        Gravity = -(float)Math.Sqrt(2f * GravityAccSpeed * height);
    }
    
    /// <summary>
    /// Movement direction projected on XZ plane, using local X basis as "horizontal" direction.
    /// </summary>
    public Vector3 GetMoveDirection() => new (MoveDirection.X, 0, MoveDirection.Y);

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

    public bool IsInWater(Vector3 offset)
    {
        return WaterOverlap.IsOverlapping(
            result => result.GetData("Water", false),
            offset,
            true);
    }
    
    public bool IsInWater(bool forceUpdate = false)
    {
        if (!forceUpdate) return InWater;

        InWater = IsInWater(Vector3.Zero);
        return InWater;
    }

    private bool OnWall = false;

    public bool IsReallyOnWall() => OnWall;

    // binary search for floating height (sampled along gravity direction)
    private const float FloatingDetectStep = 64f;
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
        Velocity = Vector3.Zero;
        var moveSpeed = 0f;
        var moveDir = GetMoveDirection();

        if (EnableGravity) { Velocity += GetGravityDirection() * Gravity; }
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

        // single pit get through (slope fix similar to 2D)
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

                GlobalTransform = GlobalTransform.Translated((float)(moveSpeed * delta) * moveDir);
                GlobalTransform = GlobalTransform.Translated(SlopeFixLength * UpDirection);
                MoveAndCollide((SlopeFixLength + 1f) * -UpDirection);
            }
        }

        // signal
        if (!onFloorLast && IsOnFloor()) EmitSignal(SignalName.FloorCollided);
        if (!onCeilingLast && IsOnCeiling()) EmitSignal(SignalName.CeilingCollided);
        if (!onWallLast && OnWall) EmitSignal(SignalName.WallCollided, moveDir);

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