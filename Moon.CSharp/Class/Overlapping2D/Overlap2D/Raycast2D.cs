using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;

namespace Moon.Class;

public abstract class Raycast2D : PhysicsQuery2D
{
    public PhysicsRayQueryParameters2D QueryParameters { get; set; } = new() 
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
    
    public IEnumerable<RaycastResult2D<T>> QueryRaycastObjects<T>(Func<RaycastResult2D<T>, bool> filter, bool excludeOthers = false) where T : GodotObject
    {
        if (!PhysicsServer2D.SpaceIsActive(Space)) yield break;
        
        var exclude = Exclude.Duplicate();
        var queried = Exclude.Duplicate();

        var count = 0;
        while (count < MaxResults)
        {
            Exclude = queried;
            var query = SpaceState.IntersectRay(QueryParameters);
            if (query is null) break;
            
            count++;
            var col = query.GetCollider();
            var rid = query.GetRid();
            queried.Add(rid);
            
            if (col is T colt)
            {
                var result = new RaycastResult2D<T>()
                {
                    Collider = colt,
                    Rid = rid,
                    Position = query.GetPosition(),
                    Normal = query.GetNormal(),
                };

                if (filter == null || filter(result))
                {
                    Exclude = exclude;
                    yield return result;
                    continue;
                }
            }
            
            if (excludeOthers)
            {
                exclude.Add(rid);
            }
        }
        
        Exclude = exclude;
    }
    
    public IEnumerable<RaycastResult2D<T>> QueryRaycastObjects<T>(bool excludeOthers = false) where T : GodotObject
        => QueryRaycastObjects<T>(null, excludeOthers);
    public IEnumerable<RaycastResult2D<GodotObject>> QueryRaycastObjects(Func<RaycastResult2D<GodotObject>, bool> filter, bool excludeOthers = false)
        => QueryRaycastObjects<GodotObject>(filter, excludeOthers);
    public IEnumerable<RaycastResult2D<GodotObject>> QueryRaycastObjects()
        => QueryRaycastObjects(null);
        
    /// <summary>
    /// Raycast parameters used to query.
    /// </summary>
    protected record struct RaycastInfo(Vector2 From, Vector2 To, bool Inside);
        
    /// <summary>
    /// The manager will call this method to do raycast query.
    /// Override to implement.
    /// </summary>
    protected abstract RaycastInfo GetRaycastInfo();
    
    private void SetRaycastInfo(RaycastInfo info, Vector2 deltaPos = default)
    {
        QueryParameters.From = info.From + deltaPos;
        QueryParameters.To = info.To + deltaPos;
        QueryParameters.HitFromInside = info.Inside;
    }
    
    public IEnumerable<RaycastResult2D<T>> GetRaycastObjects<T>(Func<RaycastResult2D<T>, bool> filter, Vector2 deltaPos = default, bool excludeOthers = false) where T : GodotObject
    {
        var info = GetRaycastInfo();
        SetRaycastInfo(info, deltaPos);
        return QueryRaycastObjects(filter, excludeOthers);
    }
    
    public IEnumerable<RaycastResult2D<T>> GetRaycastObjects<T>(Vector2 deltaPos = default, bool excludeOthers = false) where T : GodotObject
        => GetRaycastObjects<T>(null, deltaPos, excludeOthers);
    public IEnumerable<RaycastResult2D<GodotObject>> GetRaycastObjects(Func<RaycastResult2D<GodotObject>, bool> filter, Vector2 deltaPos = default, bool excludeOthers = false)
        => GetRaycastObjects<GodotObject>(filter, deltaPos, excludeOthers);
    public IEnumerable<RaycastResult2D<GodotObject>> GetRaycastObjects(Vector2 deltaPos = default)
        => GetRaycastObjects<GodotObject>(null, deltaPos);
    
    public bool IsRaycast<T>(Func<RaycastResult2D<T>, bool> filter, Vector2 deltaPos = default, bool excludeOthers = false) where T : GodotObject
    {
        var info = GetRaycastInfo();
        SetRaycastInfo(info, deltaPos);
        return QueryRaycastObjects(filter, excludeOthers).Any();
    }
    
    public bool IsRaycast<T>(Vector2 deltaPos = default, bool excludeOthers = false) where T : GodotObject
        => IsRaycast<T>(null, deltaPos, excludeOthers);
    public bool IsRaycast(Func<RaycastResult2D<GodotObject>, bool> filter, Vector2 deltaPos = default, bool excludeOthers = false)
        => IsRaycast<GodotObject>(filter, deltaPos, excludeOthers);
    public bool IsRaycast(Vector2 deltaPos = default)
        => IsRaycast<GodotObject>(null, deltaPos);
    
    public bool IsRaycastWith(GodotObject col, Vector2 deltaPos = default, bool excludeOthers = false)
        => IsRaycast(result => result.Collider == col, deltaPos, excludeOthers);
    
    public bool IsRaycastWith(Rid rid, Vector2 deltaPos = default, bool excludeOthers = false)
        => IsRaycast(result => result.Rid == rid, deltaPos, excludeOthers);
}