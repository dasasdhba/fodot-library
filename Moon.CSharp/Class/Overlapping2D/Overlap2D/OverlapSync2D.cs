using System.Collections.Generic;
using Godot;

namespace Moon.Class;

/// <summary>
/// Overlap system that handles overlapping test.
/// Share the same CollisionShape2D with target CollisionObject2D.
/// </summary>
public class OverlapSync2D : Overlap2D
{
    public static OverlapSync2D CreateFrom(CollisionObject2D col)
    {
        var result = new OverlapSync2D();
        result.Bind(col);
        result.SyncObject = col;
        result.CollisionMask = col.CollisionMask;
        result.AddException(col);
        return result;
    }
    
    /// <summary>
    /// The target CollisionObject2D to sync with.
    /// </summary>
    public CollisionObject2D SyncObject { get; set; }

    protected override IEnumerable<ShapeInfo> GetShapeInfos()
    {
        if (GodotObject.IsInstanceValid(SyncObject))
        {
            foreach (var i in SyncObject.GetShapeOwners())
            {
                var ui = (uint)i;
                if (SyncObject.IsShapeOwnerDisabled(ui)) { continue; }

                var owner = (Node2D)SyncObject.ShapeOwnerGetOwner(ui);
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