using System;
using Godot;
using Moon.Utils;

namespace Moon.Class;

// RigidBody provides a much more stable physics simulation than CharacterBody.
// using round rectangle shape (rectangle + capsule) is recommended
// this class tries to simplify the manipulation of rigid bodies

[GlobalClass]
public partial class RigidPlatformer2D : RigidBody2D, IPlatformer2D
{
    [ExportCategory("RigidPlatformer2D")]
    [Export]
    public int MaxContacts
    {
        get => _MaxContacts;
        set
        {
            _MaxContacts = value;
            MaxContactsReported = value;
        }
    }
    private int _MaxContacts = 32;
    
    [Export]
    public float SlopeMaxAngle { get; set; } = 45f;
    
    [ExportGroup("Gravity", "Gravity")]
    [Export]
    public float GravityGeneralScale { get; set; } = 1f;

    [Export] 
    public float GravityMaxSpeed { get; set; } = 500f;

    [Export] 
    public float GravityDecRate { get; set; } = 1.5f;

    [Export]
    public float GravityWaterScale { get; set; } = 0.25f;

    [Export] 
    public float GravityWaterMaxSpeed { get; set; } = 150f;

    [Export] 
    public float GravityWaterDecRate { get; set; } = 5f;

    [Export] 
    public float GravityFloatingHeight { get; set; } = -1f;
    
    [Export]
    public float GravityFloatingSpeed { get; set; } = 1000f;
    
    [Export]
    public float GravityFloatingDamp { get; set; } = 4f;
    
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
    public delegate void RigidFrameEventHandler(PhysicsDirectBodyState2D state, double delta);

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
    
    public virtual void RigidProcess(PhysicsDirectBodyState2D state, double delta) { }
    
    protected OverlapSync2D WaterOverlap { get; set; }

    public RigidPlatformer2D() : base()
    {
        // enable contact for floor and ceiling detection
            
        ContactMonitor = true;
        MaxContactsReported = MaxContacts;
    
        Ready += () =>
        {
            // water detection
            
            WaterOverlap = OverlapSync2D.CreateFrom(this);
            WaterOverlap.CollisionMask = WaterMask;
        };
    }

    private bool OnFloor;
    public bool IsOnFloor() => OnFloor;

    private bool OnCeiling;
    public bool IsOnCeiling() => OnCeiling;

    private bool OnWall;
    public bool IsOnWall() => OnWall;
    
    // just for interface
    public bool IsReallyOnWall() => OnWall;

    public bool IsInWater(Vector2 offset)
    {
        return WaterOverlap.IsOverlapping(
            result => result.GetData("Water", false),
            offset,
            true);
    }

    private bool InWater;
    public bool IsInWater(bool forceUpdate = false)
    {
        if (!forceUpdate) return InWater;

        InWater = IsInWater(Vector2.Zero);
        return InWater;
    }
    
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
    
    /// <summary>
    /// Warning: GetGravity() is not valid until physics init
    /// </summary>
    public Vector2 GetGravityDirection() => GetGravity().Normalized();
    
    public void SetGravitySpeed(float speed)
    {
        SignalOneshotRigidFrame += (state, delta) =>
        {
            var vec = speed * GetGravityDirection();
            state.LinearVelocity = state.LinearVelocity.WithAxis(vec);
        };
    }
    public float GetGravitySpeed() => LinearVelocity.Dot(GetGravityDirection());
    
    public void Jump(float height)
    {
        // v^2 = 2ax
        var g = GetGravity();
        SetGravitySpeed(-(float)Math.Sqrt(2f * g.Length() * height));
    }
    
    public Vector2 GetMoveDirection() => GetGravityDirection().Orthogonal();

    /// <summary>
    /// The real speed will be a little slower than the set one
    /// due to the physics simulation of friction.
    /// Setting friction to 0 is OK but we'll lose the simulation of moving blocks.
    /// </summary>
    public void SetMoveSpeed(float speed, bool updatePhysics = false)
    {
        SignalOneshotRigidFrame += (state, delta) =>
        {
            var dir = GetMoveDirection();
            var vec = speed * dir;
            
            // we apply force instead of velocity for better stability
            // since this method is often called in process
            
            var last = GetLastMoveSpeed();
            state.ApplyCentralForce((float)((speed - last) / delta) * dir);
            
            if (updatePhysics) LastVelocity = LastVelocity.WithAxis(vec);
        };
    }
    public float GetMoveSpeed() => LastVelocity.Dot(GetMoveDirection());
    
    private Vector2 LastMotion;
    public Vector2 GetLastMotion() => LastMotion;

    private Vector2 LastVelocity;
    public Vector2 GetLastVelocity() => LastVelocity;

    private Vector2 LastGravity;
    public Vector2 GetLastGravity() => LastGravity;
    public Vector2 GetLastGravityDirection() => LastGravity.Normalized();
    public float GetLastGravitySpeed() => GetLastVelocity().Dot(GetLastGravityDirection());
    public float GetLastMoveSpeed() => GetLastVelocity().Dot(GetLastGravityDirection().Orthogonal());

    private Vector2 LastPosition;
    
    private bool LastOnFloor;
    private bool LastOnCeiling;
    private bool LastOnWall;
    
    private bool FirstProcessed;
    public override void _IntegrateForces(PhysicsDirectBodyState2D state)
    {
        var delta = GetPhysicsProcessDeltaTime();

        // last infos

        LastVelocity = state.LinearVelocity;
        LastGravity = state.TotalGravity;
        var lastGravityDir = LastGravity.Normalized();
        var lastGravitySpeed = LastVelocity.Dot(lastGravityDir);

        if (FirstProcessed)
        {
            // collision update
            
            OnFloor = false;
            OnCeiling = false;
            OnWall = false;
            
            // warning: we need a more precise collision detection here
            // contact reports are proven to be unreliable
            
            // rigid body with rectangle shape often get stuck here
            // a capsule bottom is recommended
            
            var moveSign = 0;
            
            for (int i = 0; i < state.GetContactCount(); i++)
            {
                var n = state.GetContactLocalNormal(i);
                
                var gProj = n.Dot(lastGravityDir);
                if (!Mathf.IsZeroApprox(gProj))
                {
                    if (gProj > 0f) OnCeiling = true;
                    else OnFloor = true;
                }

                if (Math.Abs(Mathf.AngleDifference(n.Angle(), (-LastGravity).Angle()))
                     > Mathf.DegToRad(SlopeMaxAngle))
                {
                    OnWall = true;
                    moveSign = -Math.Sign(n.Dot(GetMoveDirection()));
                }
            }
            
            if (OnFloor && !LastOnFloor) EmitSignal(SignalName.FloorCollided);
            if (OnCeiling && !LastOnCeiling) EmitSignal(SignalName.CeilingCollided);
            if (OnWall && !LastOnWall) EmitSignal(SignalName.WallCollided, moveSign);
        
            // water update
        
            var inWaterLast = IsInWater();
            IsInWater(true);
        
            if (InWater && !inWaterLast) EmitSignal(SignalName.WaterEntered);
            if (!InWater && inWaterLast) EmitSignal(SignalName.WaterExited);
            
            // motion update
            
            LastMotion = GlobalPosition - LastPosition;
        }
        else
        {
            FirstProcessed = true;
            IsInWater(true);
        }
        
        // record last
        
        LastOnFloor = OnFloor;
        LastOnCeiling = OnCeiling;
        LastOnWall = OnWall;
        LastPosition = GlobalPosition;

        // water update

        GravityScale = InWater ? GravityWaterScale : GravityGeneralScale;
        
        if (InWater && GravityFloatingHeight >= 0f)
        {
            // floating
            
            LinearDamp = GravityFloatingDamp;
            
            var d = GetFloatingHeight() - GravityFloatingHeight;
            var dir = -GetGravityDirection();
            state.ApplyCentralForce((float)(d * GravityFloatingSpeed * delta) * dir);
        }
        else
        {
            // limit max speed
            
            LinearDamp = 0f;

            var max = InWater ? GravityWaterMaxSpeed : GravityMaxSpeed;
            var dec = InWater ? GravityWaterDecRate : GravityDecRate;
            var g = GetGravity();
            var len = g.Length();
            max -= (float)(len * delta);
            if (lastGravitySpeed > max)
            {
                var dir = -g.Normalized();
                len = (float)Math.Min(len * dec, (lastGravitySpeed - max) / delta);
                state.ApplyCentralForce(len * dir);
            }
        }

        // custom process

        EmitSignal(SignalName.RigidFrame, state, delta);
        RigidProcess(state, delta);
    }
}