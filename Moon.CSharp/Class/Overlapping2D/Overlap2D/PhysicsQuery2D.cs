using Godot;
using Godot.Collections;

namespace Moon.Class;

/// <summary>
/// Store physics space and direct state.
/// Provide basic query parameters.
/// </summary>
public class PhysicsQuery2D
{
    // collision mask

    public virtual uint CollisionMask { get ;set; }
    
    public bool GetCollisionMaskValue(int layer)
        => ((CollisionMask >> (layer - 1)) & 1) == 1;

    public void SetCollisionMaskValue(int layer, bool value)
    {
        int n = layer - 1;
        int mask = (int)CollisionMask;
        if (value) mask |= 1 << n;
        else mask &= ~(1 << n);
        CollisionMask = (uint)mask;
    }
    
    // exceptions
    
    protected virtual Array<Rid> Exclude { get ;set; } 
    
    public void ClearException() => Exclude = new();

    public void AddException(Rid rid)
    {
        var arr = Exclude;
        arr.Add(rid);
        Exclude = arr;
    }

    public void AddException(CollisionObject2D obj)
    {
        AddException(obj.GetRid());
    }

    public void RemoveException(Rid rid)
    {
        var arr = Exclude;
        arr.Remove(rid);
        Exclude = arr;
    }
    public void RemoveException(CollisionObject2D obj) => RemoveException(obj.GetRid());

    // space
    
    protected Rid Space { get ;set; }
    protected PhysicsDirectSpaceState2D SpaceState { get ;set ;}

    public PhysicsQuery2D() { }

    /// <summary>
    /// Construct with rid.
    /// </summary>
    /// <param name="space">The RID of space.</param>
    public PhysicsQuery2D(Rid space) => SetSpace(space);

    /// <summary>
    /// Construct with node, using the node's space as the physics space.
    /// </summary>
    /// <param name="node">The node to query space.</param>
    public PhysicsQuery2D(Node node) => Bind(node);

    /// <summary>
    /// Change space.
    /// </summary>
    /// <param name="space">The RID of space.</param>
    public void SetSpace(Rid space)
    {
        Space = space;
        SpaceState = PhysicsServer2D.SpaceGetDirectState(Space);
    }

    /// <summary>
    /// Using the node's space as the physics space.
    /// </summary>
    /// <param name="node">The node to query space.</param>
    public void Bind(Node node)
    {
        if (node.IsInsideTree()) SetSpaceInTree(node);
        node.TreeEntered += () => SetSpaceInTree(node);
    }

    private void SetSpaceInTree(Node node)
        => SetSpace(node.GetViewport().FindWorld2D().Space);
}