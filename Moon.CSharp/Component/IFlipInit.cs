using Godot;
using Moon.Class;
using Moon.Utils;

namespace Moon.Component;

public interface IFlipHInit
{
    public void FlipHInit();
}

public interface IFlipVInit
{
    public void FlipVInit();
}


public static class FlipExtensions
{
    /// <summary>
    /// Flip All possible nodes when init
    /// </summary>
    /// <param name="node"></param>
    public static void InitFlipH(this Node node)
    {
        TryInitFlip(node);
        node.SetChildrenRecursively(TryInitFlip);
        
        void TryInitFlip(Node node)
        {
            if (node is IFlipHInit flip) flip.FlipHInit();
        }
    }
    
    public static void InitFlipV(this Node node)
    {
        TryInitFlip(node);
        node.SetChildrenRecursively(TryInitFlip);
        
        void TryInitFlip(Node node)
        {
            if (node is IFlipVInit flip) flip.FlipVInit();
        }
    }

    public static bool TryGetFlipH(this Node node)
    {
        var flip = node.Get(Sprite2D.PropertyName.FlipH);
        if (flip.VariantType != Variant.Type.Nil)
        {
            return flip.AsBool();
        }
        flip = node.Get(AnimGroup2D.PropertyName.FlipH);
        if (flip.VariantType != Variant.Type.Nil)
        {
            return flip.AsBool();
        }
        
        return false;
    }
    
    public static void TrySetFlipH(this Node node, bool value)
    {
        var flip = node.Get(Sprite2D.PropertyName.FlipH);
        if (flip.VariantType != Variant.Type.Nil)
        {
            node.Set(Sprite2D.PropertyName.FlipH, value);
        }
        flip = node.Get(AnimGroup2D.PropertyName.FlipH);
        if (flip.VariantType != Variant.Type.Nil)
        {
            node.Set(AnimGroup2D.PropertyName.FlipH, value);
        }
    }
    
    public static bool TryGetFlipV(this Node node)
    {
        var flip = node.Get(Sprite2D.PropertyName.FlipV);
        if (flip.VariantType != Variant.Type.Nil)
        {
            return flip.AsBool();
        }
        flip = node.Get(AnimGroup2D.PropertyName.FlipV);
        if (flip.VariantType != Variant.Type.Nil)
        {
            return flip.AsBool();
        }
        
        return false;
    }
    
    public static void TrySetFlipV(this Node node, bool value)
    {
        var flip = node.Get(Sprite2D.PropertyName.FlipV);
        if (flip.VariantType != Variant.Type.Nil)
        {
            node.Set(Sprite2D.PropertyName.FlipV, value);
        }
        flip = node.Get(AnimGroup2D.PropertyName.FlipV);
        if (flip.VariantType != Variant.Type.Nil)
        {
            node.Set(AnimGroup2D.PropertyName.FlipV, value);
        }
    }
}