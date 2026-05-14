using System;
using Godot;

namespace Moon.Class;

public interface IPlatformer2D
{
    // custom godot signal is specific event handler
    // so it cannot be interfaced, weary
    // we use our self defined System.Action event here
    
    public event Action SignalFloorCollided;
    public event Action SignalOneshotFloorCollided;
    public event Action SignalCeilingCollided;
    public event Action SignalOneshotCeilingCollided;
    public event Action<int> SignalWallCollided;
    public event Action<int> SignalOneshotWallCollided;
    public event Action SignalWaterEntered;
    public event Action SignalOneshotWaterEntered;
    public event Action SignalWaterExited;
    public event Action SignalOneshotWaterExited;

    public bool IsOnFloor();
    public bool IsOnCeiling();
    public bool IsOnWall();
    public bool IsReallyOnWall();
    public bool IsInWater(bool forceUpdate = false);
    public bool IsInWater(Vector2 offset);
    
    public Vector2 GetGravityDirection();
    public void SetGravitySpeed(float speed);
    public void Jump(float height);
    
    public Vector2 GetMoveDirection();
    public void SetMoveSpeed(float speed, bool updatePhysics = false);
    
    public Vector2 GetLastMotion();
    public float GetLastGravitySpeed();
    public float GetLastMoveSpeed();
    public float GetGravitySpeed();
    public float GetMoveSpeed();
}