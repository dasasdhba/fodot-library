using System.Collections.Generic;
using Godot;

namespace Moon.Class;

/// <summary>
/// Overlap system that handles overlapping test (3D).
/// Share the same CollisionShape3D with target CollisionObject3D.
/// </summary>
public class OverlapSync3D : Overlap3D
{
    public static OverlapSync3D CreateFrom(CollisionObject3D col)
    {
        var result = new OverlapSync3D();
        result.Bind(col);
        result.SyncObject = col;
        result.CollisionMask = col.CollisionMask;
        result.AddException(col);
        return result;
    }
    
    /// <summary>
    /// The target CollisionObject3D to sync with.
    /// </summary>
    public CollisionObject3D SyncObject { get; set; }

    protected override IEnumerable<ShapeInfo> GetShapeInfos()
    {
        if (GodotObject.IsInstanceValid(SyncObject))
        {
            foreach (var i in SyncObject.GetShapeOwners())
            {
                var ui = (uint)i;
                if (SyncObject.IsShapeOwnerDisabled(ui)) { continue; }

                var owner = (Node3D)SyncObject.ShapeOwnerGetOwner(ui);
                var transform = owner.GlobalTransform;

                for (int j = 0; j < SyncObject.ShapeOwnerGetShapeCount(ui); j++)
                {
                    yield return new ShapeInfo(
                        SyncObject.ShapeOwnerGetShape(ui,j),
                        transform
                        );
                }
            }
        }
    }
}