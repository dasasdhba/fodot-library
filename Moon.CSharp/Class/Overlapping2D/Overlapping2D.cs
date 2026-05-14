using System;
using System.Collections.Generic;
using Godot;

namespace Moon.Class;

/// <summary>
/// Base class of OverlappingObjects.
/// Using Component.OverlapManager2D object as base API.
/// </summary>
[GlobalClass]
public abstract partial class Overlapping2D : Node
{

    /// <summary>
    /// The manager that needs to override.
    /// </summary>
    protected abstract Overlap2D GetOverlapManager();

    [ExportCategory("Overlapping2D")]

    [Export]
    public bool CollideWithAreas
    {
        get => _CollideWithAreas;
        set
        {
            _CollideWithAreas = value;
            GetOverlapManager().CollideWithAreas = value;
        }
    }
    private bool _CollideWithAreas = false;

    [Export]
    public bool CollideWithBodies
    {
        get => _CollideWithBodies;
        set
        {
            _CollideWithBodies = value;
            GetOverlapManager().CollideWithBodies = value;
        }
    }
    private bool _CollideWithBodies = true;

    [Export]
    public float Margin
    {
        get => _Margin;
        set
        {
            _Margin = value;
            GetOverlapManager().Margin = value;
        }
    }
    private float _Margin = 0f;

    [Export]
    public int MaxResults
    {
        get => _MaxResults;
        set
        {
            _MaxResults = value;
            GetOverlapManager().MaxResults = value;
        }
    }
    private int _MaxResults = 32;

    [Export(PropertyHint.Layers2DPhysics)]
    public uint CollisionMask
    {
        get => _CollisionMask;
        set
        {
            _CollisionMask = value;
            GetOverlapManager().CollisionMask = value;
        }
    }
    private uint _CollisionMask = 1;

    // init
    public Overlapping2D() : base()
    {
        TreeEntered += () =>
        {
            var manager = GetOverlapManager();
            manager.CollideWithAreas = CollideWithAreas;
            manager.CollideWithBodies = CollideWithBodies;
            manager.Margin = Margin;
            manager.MaxResults = MaxResults;
            manager.CollisionMask = CollisionMask;
            manager.Bind(this);
        };
    }

    // exposed APIs

    public bool GetCollisionMaskValue(int layer)
        => GetOverlapManager().GetCollisionMaskValue(layer);

    public void SetCollisionMaskValue(int layer, bool value)
        => GetOverlapManager().SetCollisionMaskValue(layer, value);

    // exception
    public void ClearException() => GetOverlapManager().ClearException();
    public void AddException(CollisionObject2D col) => GetOverlapManager().AddException(col);
    public void AddException(Rid rid) => GetOverlapManager().AddException(rid);
    public void RemoveException(CollisionObject2D col) => GetOverlapManager().RemoveException(col);
    public void RemoveException(Rid rid) => GetOverlapManager().RemoveException(rid);
    
    public IEnumerable<OverlapResult2D<GodotObject>> GetOverlappingObjects(Func<OverlapResult2D<GodotObject>, bool> filter, Vector2 deltaPos = default, bool excludeOthers = false)
        => GetOverlapManager().GetOverlappingObjects(filter, deltaPos, excludeOthers);
    
    public IEnumerable<OverlapResult2D<GodotObject>> GetOverlappingObjects(Vector2 deltaPos = default)
        => GetOverlapManager().GetOverlappingObjects(deltaPos);
    
    public IEnumerable<OverlapResult2D<T>> GetOverlappingObjects<T>(Func<OverlapResult2D<T>, bool> filter, Vector2 deltaPos = default, bool excludeOthers = false) where T : GodotObject
        => GetOverlapManager().GetOverlappingObjects(filter, deltaPos, excludeOthers);
        
    public IEnumerable<OverlapResult2D<T>> GetOverlappingObjects<T>(Vector2 deltaPos = default, bool excludeOthers = false) where T : GodotObject
        => GetOverlapManager().GetOverlappingObjects<T>(deltaPos, excludeOthers);
    
    public bool IsOverlapping<T>(Func<OverlapResult2D<T>, bool> filter, Vector2 deltaPos = default, bool excludeOthers = false) where T : GodotObject
        => GetOverlapManager().IsOverlapping(filter, deltaPos, excludeOthers);
        
    public bool IsOverlapping<T>(Vector2 deltaPos = default, bool excludeOthers = false) where T : GodotObject
        => GetOverlapManager().IsOverlapping<T>(deltaPos, excludeOthers);
        
    public bool IsOverlapping(Func<OverlapResult2D<GodotObject>, bool> filter, Vector2 deltaPos = default, bool excludeOthers = false)
        => GetOverlapManager().IsOverlapping(filter, deltaPos, excludeOthers);
        
    public bool IsOverlapping(Vector2 deltaPos = default)
        => GetOverlapManager().IsOverlapping(deltaPos);
    
    public bool IsOverlappingWith(GodotObject col, Vector2 deltaPos = default, bool excludeOthers = false)
        => GetOverlapManager().IsOverlappingWith(col, deltaPos, excludeOthers);
        
    public bool IsOverlappingWith(Rid rid, Vector2 deltaPos = default, bool excludeOthers = false)
        => GetOverlapManager().IsOverlappingWith(rid, deltaPos, excludeOthers);
}