using System;
using Godot;

namespace Moon.Utils;

public static class FodotExtensions
{
    public static T InstantiateSafely<T>(this PackedScene scene) where T : Node
    {
        return Fodot.Core.PackedScene.instantiateTo<T>(scene);
    }

    public static void AddChildSafely(this Node node, Node child, Node.InternalMode internalMode = Node.InternalMode.Disabled)
    {
        Fodot.Core.Node.addChildInternal(child, internalMode, node);
    }
    
    public static void AddSiblingSafely(this Node node, Node sibling)
    {
        Fodot.Core.Node.addSibling(sibling, node);
    }

    public static void MoveChildSafely(this Node node, Node child, int index)
    {
        Fodot.Core.Node.moveChild(child, index, node);
    }
    
    public static void RemoveChildSafely(this Node node, Node child)
    {
        Fodot.Core.Node.removeChild(child, node);
    }

    public static void ReparentSafely(this Node node, Node parent, bool keepTransform = true)
    {
        Fodot.Core.Node.reparent(parent, keepTransform, node);
    }

    public static void BindChild(this Node node, Node child)
    {
        Fodot.Core.Node.bindChild(child, node);
    }
    
    public static NodePath GetUniquePath(this Node node)
        => Module.Node.getUniquePath(node);
        
    public static Node GetCurrentScene(this Node node)
        => Fodot.Stage.Node.getCurrentScene(node);

    public static T FindParent<T>(this Node node, Func<T, bool> filter = null) where T : Node
    {
        if (filter == null) return Fodot.Module.Node.findParent<T>(node).Value;
        return Fodot.Module.Node.findParentWith(filter.AsFSharpFunc(), node).Value;
    }
}