using Godot;

namespace Moon.Class;

/// <summary>
/// A light weighted version of RayCast2D Node
/// </summary>
public class RaycastSync2D : Raycast2D
{
    public static RaycastSync2D CreateFrom(CollisionObject2D col)
    {
        var result = new RaycastSync2D();
        result.SyncObject = col;
        result.Bind(col);
        result.CollisionMask = col.CollisionMask;
        result.AddException(col);
        return result;
    }
    
    public static RaycastSync2D CreateFrom(Node2D origin, CollisionObject2D col)
    {
        var result = new RaycastSync2D();
        result.SyncObject = origin;
        result.Bind(col);
        result.CollisionMask = col.CollisionMask;
        result.AddException(col);
        return result;
    }
    
    /// <summary>
    /// The target Node2D to sync with.
    /// </summary>
    public Node2D SyncObject { get; set; }
    public Vector2 Target { get ;set; }
    public bool HitFromInside { get ;set; } = false;

    protected override RaycastInfo GetRaycastInfo()
    {
        return new()
        {
            From = SyncObject.GlobalPosition,
            To = SyncObject.ToGlobal(Target),
            Inside = HitFromInside,
        };
    }
}