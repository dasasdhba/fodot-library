using System;
using System.Collections.Generic;
using Godot;

namespace Fodot.CSharp;

public static class FodotExtensions
{
    public static T InstantiateSafely<T>(this PackedScene scene) where T : Node
    {
        return PackedSceneModule.instantiateTo<T>(scene);
    }

    public static void AddChildSafely(this Node node, Node child, Node.InternalMode internalMode = Node.InternalMode.Disabled)
    {
        NodeModule.addChildInternal(child, internalMode, node);
    }
    
    public static void AddSiblingSafely(this Node node, Node sibling)
    {
        NodeModule.addSibling(sibling, node);
    }

    public static void MoveChildSafely(this Node node, Node child, int index)
    {
        NodeModule.moveChild(child, index, node);
    }
    
    public static void RemoveChildSafely(this Node node, Node child)
    {
        NodeModule.removeChild(child, node);
    }

    public static void ReparentSafely(this Node node, Node parent, bool keepTransform = true)
    {
        NodeModule.reparent(parent, keepTransform, node);
    }

    public static void BindChild(this Node node, Node child)
    {
        NodeModule.bindChild(child, node);
    }
    
    public static T FindParent<T>(this Node node, Func<T, bool> filter = null) where T : Node
    {
        if (filter == null) return Extend.Node.findParent<T>(node).Value;
        return Extend.Node.findParentWith(filter.AsFSharpFunc(), node).Value;
    }
    
    public static IEnumerable<T> GetChildren<T>(this Node node, bool includeInternal = false) where T : Node
    {
        return NodeModule.getChildrenInternalOrNot<T>(includeInternal, node);
    }
    
    public static IEnumerable<T> GetChildrenRec<T>(this Node node, bool includeInternal = false) where T : Node
    {
        return NodeModule.getChildrenRecInternalOrNot<T>(includeInternal, node);
    }
}
