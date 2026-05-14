using Godot;

namespace Moon.Class;

/// <summary>
/// A light weighted version of RayCast3D Node
/// </summary>
public class RaycastSync3D : Raycast3D
{
    public static RaycastSync3D CreateFrom(CollisionObject3D col)
    {
        var result = new RaycastSync3D();
        result.SyncObject = col;
        result.Bind(col);
        result.CollisionMask = col.CollisionMask;
        result.AddException(col);
        return result;
    }
    
    public static RaycastSync3D CreateFrom(Node3D origin, CollisionObject3D col)
    {
        var result = new RaycastSync3D();
        result.SyncObject = origin;
        result.Bind(col);
        result.CollisionMask = col.CollisionMask;
        result.AddException(col);
        return result;
    }
    
    /// <summary>
    /// The target Node3D to sync with.
    /// </summary>
    public Node3D SyncObject { get; set; }
    public Vector3 Target { get ;set; }
    public bool HitFromInside { get ;set; } = false;

    protected override RaycastInfo GetRaycastInfo()
    {
        return new()
        {
            From = SyncObject.GlobalTransform.Origin,
            To = SyncObject.ToGlobal(Target),
            Inside = HitFromInside,
        };
    }
}