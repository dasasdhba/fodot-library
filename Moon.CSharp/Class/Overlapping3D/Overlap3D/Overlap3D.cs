using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;

namespace Moon.Class;

/// <summary>
/// Base class of overlapping system (3D).
/// Do overlapping query through Godot Physics Server 3D.
/// </summary>
public abstract class Overlap3D : PhysicsQuery3D
{
    public PhysicsShapeQueryParameters3D QueryParameters { get; set; } = new() 
    { 
        CollisionMask = 1,
        Exclude = new()
    };
    public int MaxResults { get; set; } = 32;

    public bool CollideWithAreas
    {
        get => QueryParameters.CollideWithAreas;
        set => QueryParameters.CollideWithAreas = value;
    }

    public bool CollideWithBodies
    {
        get => QueryParameters.CollideWithBodies;
        set => QueryParameters.CollideWithBodies = value;
    }

    public float Margin
    {
        get => QueryParameters.Margin;
        set => QueryParameters.Margin = value;
    }

    public override uint CollisionMask
    {
        get => QueryParameters.CollisionMask;
        set => QueryParameters.CollisionMask = value;
    }

    protected override Array<Rid> Exclude
    {
        get => QueryParameters.Exclude;
        set => QueryParameters.Exclude = value;
    }

    public IEnumerable<OverlapResult3D<T>> QueryOverlappingObjects<T>(Func<OverlapResult3D<T>, bool> filter, bool excludeOthers = false) where T : GodotObject
    {
        if (!PhysicsServer3D.SpaceIsActive(Space)) yield break;

        var query = SpaceState.IntersectShape(QueryParameters, MaxResults);
        for (int i = 0; i < query.GetCollisionCount(); i++)
        {
            var col = query.GetCollider(i);
            if (col is T colt)
            {
                var result = new OverlapResult3D<T>()
                {
                    Collider = colt,
                    Rid = query.GetRid(i),
                };

                if (filter == null || filter(result))
                {
                    yield return result;
                    continue;
                }
            }
            
            if (excludeOthers)
            {
                AddException(query.GetRid(i));
            }
        }
    }
    
    public IEnumerable<OverlapResult3D<T>> QueryOverlappingObjects<T>(bool excludeOthers = false) where T : GodotObject
        => QueryOverlappingObjects<T>(null, excludeOthers);
    public IEnumerable<OverlapResult3D<GodotObject>> QueryOverlappingObjects(Func<OverlapResult3D<GodotObject>, bool> filter, bool excludeOthers = false)
        => QueryOverlappingObjects<GodotObject>(filter, excludeOthers);
    public IEnumerable<OverlapResult3D<GodotObject>> QueryOverlappingObjects()
        => QueryOverlappingObjects(null);
        
    /// <summary>
    /// Shape3D and its (Global) Transform used to query.
    /// </summary>
    protected record struct ShapeInfo(Shape3D Shape, Transform3D Transform);

    /// <summary>
    /// The manager will call this method to do overlapping query.
    /// Override to implement.
    /// </summary>
    protected abstract IEnumerable<ShapeInfo> GetShapeInfos();

    private void SetShapeInfo(ShapeInfo info, Vector3 deltaPos = default)
    {
        QueryParameters.Shape = info.Shape;
        QueryParameters.Transform = info.Transform with
        {
            Origin = info.Transform.Origin + deltaPos
        };
    }
    
    public IEnumerable<OverlapResult3D<T>> GetOverlappingObjects<T>(Func<OverlapResult3D<T>, bool> filter, Vector3 deltaPos = default, bool excludeOthers = false) where T : GodotObject
    {
        var infos = GetShapeInfos().ToArray();
        if (infos.Length == 1)
        {
            SetShapeInfo(infos[0], deltaPos);
            foreach (var result in QueryOverlappingObjects(filter, excludeOthers))
            {
                yield return result;
            }
            
            yield break;
        }

        var hash = new HashSet<OverlapResult3D<T>>();
        foreach (var info in infos)
        {
            SetShapeInfo(info, deltaPos);
            foreach (var result in QueryOverlappingObjects(filter, excludeOthers))
            {
                if (hash.Add(result))
                {
                    yield return result;
                }
            }
        }
    }
    
    public IEnumerable<OverlapResult3D<T>> GetOverlappingObjects<T>(Vector3 deltaPos = default, bool excludeOthers = false) where T : GodotObject
        => GetOverlappingObjects<T>(null, deltaPos, excludeOthers);
    public IEnumerable<OverlapResult3D<GodotObject>> GetOverlappingObjects(Func<OverlapResult3D<GodotObject>, bool> filter, Vector3 deltaPos = default, bool excludeOthers = false)
        => GetOverlappingObjects<GodotObject>(filter, deltaPos, excludeOthers);
    public IEnumerable<OverlapResult3D<GodotObject>> GetOverlappingObjects(Vector3 deltaPos = default)
        => GetOverlappingObjects<GodotObject>(null, deltaPos);
    
    public bool IsOverlapping<T>(Func<OverlapResult3D<T>, bool> filter, Vector3 deltaPos = default, bool excludeOthers = false) where T : GodotObject
    {
        foreach (var info in GetShapeInfos())
        {
            SetShapeInfo(info, deltaPos);
            if (QueryOverlappingObjects(filter, excludeOthers).Any())
            {
                return true;
            }
        }

        return false;
    }
    
    public bool IsOverlapping<T>(Vector3 deltaPos = default, bool excludeOthers = false) where T : GodotObject
        => IsOverlapping<T>(null, deltaPos, excludeOthers);
    public bool IsOverlapping(Func<OverlapResult3D<GodotObject>, bool> filter, Vector3 deltaPos = default, bool excludeOthers = false)
        => IsOverlapping<GodotObject>(filter, deltaPos, excludeOthers);
    public bool IsOverlapping(Vector3 deltaPos = default)
        => IsOverlapping<GodotObject>(null, deltaPos);
    
    public bool IsOverlappingWith(GodotObject col, Vector3 deltaPos = default, bool excludeOthers = false)
        => IsOverlapping(result => result.Collider == col, deltaPos, excludeOthers);
    
    public bool IsOverlappingWith(Rid rid, Vector3 deltaPos = default, bool excludeOthers = false)
        => IsOverlapping(result => result.Rid == rid, deltaPos, excludeOthers);
}