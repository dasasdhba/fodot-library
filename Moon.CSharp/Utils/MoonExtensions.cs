using Godot;
using System;
using System.Collections.Generic;
using Godot.Collections;

#pragma warning disable CS0618 // Type or member is obsolete

namespace Moon.Utils;

// useful node extension functions

public static class MoonExtensions
{
    #region Collections

    public static void AddNode<T>(this ICollection<T> arr, T node) where T : Node
    {
        node.TreeEntered += () => arr.Add(node);
        node.TreeExited += () => arr.Remove(node);
        if (node.IsInsideTree()) arr.Add(node);
    }

    #endregion
    
    #region DirAccess

    public static IEnumerable<string> GetFilePaths(this DirAccess dir, Func<string, bool> filter = null)
    {
        var root = dir.GetCurrentDir();
        foreach (var file in dir.GetFiles())
        {
            if (filter != null && !filter(file)) continue;
            yield return root + "/" + file;
        }
    }
    
    public static IEnumerable<string> GetFilePathsRecursively(this DirAccess dir, Func<string, bool> filter = null)
    {
        var root = dir.GetCurrentDir();
        foreach (var file in dir.GetFilePaths(filter)) yield return file;
        foreach (var sub in dir.GetDirectories())
        {
            using var subDir = DirAccess.Open(root + "/" + sub);
            foreach (var file in subDir.GetFilePathsRecursively(filter)) yield return file;
        }
    }
    
    #endregion

    #region ConfigFile

    public static System.Collections.Generic.Dictionary<string, Variant> GetSection(this ConfigFile config, string section)
    {
        var result = new System.Collections.Generic.Dictionary<string, Variant>();
        foreach (var key in config.GetSectionKeys(section))
        {
            result[key] = config.GetValue(section, key);
        }
        return result;
    }

    public static void SetSection(this ConfigFile config, string section, System.Collections.Generic.Dictionary<string, Variant> values)
    {
        foreach (var key in values.Keys)
        {
            config.SetValue(section, key, values[key]);
        }
    }

    #endregion

    #region Node

    public static Tween CreatePhysicsTween(this Node node)
    {
        var tween = node.CreateTween();
        tween.SetProcessMode(Tween.TweenProcessMode.Physics);
        return tween;
    }
    
    public static IEnumerable<T> GetChildren<T>(this Node node, 
        bool includeInternal = false) where T : Node
    {
        foreach (var child in node.GetChildren(includeInternal))
        {
            if (child is T t) yield return t;
        }
    }

    public static IEnumerable<Node> GetChildrenRecursively(this Node node, bool includeInternal = false)
    {
        foreach (var child in node.GetChildren(includeInternal))
        {
            yield return child;
            foreach (var c in child.GetChildrenRecursively(includeInternal))
            {
                yield return c;
            }
        }
    }
    
    public static IEnumerable<T> GetChildrenRecursively<T>(this Node node, 
        bool includeInternal = false) where T : Node
    {
        foreach (var child in node.GetChildrenRecursively(includeInternal))
        {
            if (child is T t) yield return t;
        }
    }

    public static void SetChildrenRecursively(this Node node, Action<Node> action, bool includeInternal = false)
    {
        foreach (var child in node.GetChildren(includeInternal))
        {
            action?.Invoke(child);
            SetChildrenRecursively(child, action, includeInternal);
        }
    }
    
    private const string ChildrenCacheTag = "_MCCache";
    private const string ChildrenRecursivelyCacheTag = "_MCRCache";
    private static void ClearChildrenCache(this GodotObject node, string tag)
    {
        foreach (string meta in node.GetMetaList())
        {
            if (meta.StartsWith(tag))
            {
                node.RemoveMeta(meta);
            }
        }
    }

    private static void SetChildrenCacheMonitor(this Node node, Node target, string tag)
    {
        var signalTag = $"{tag}_MSignal";
        if (target.HasData(signalTag))
        {
            var arr = target.GetData<Array<Node>>(signalTag);
            if (arr.Contains(node)) return;
            arr.Add(node);
        }
        else
        {
            target.SetData(signalTag, 
            new Array<Node> { node });
        }
        
        target.ChildEnteredTree += c => ClearChildrenCache(node, tag);
        target.ChildExitingTree += c => ClearChildrenCache(node, tag);
    }

    public static IEnumerable<T> GetChildrenCached<[MustBeVariant] T>(this Node node, 
        string tag = "Default", bool includeInternal = false) where T : Node
    {
    #if TOOLS
        if (Engine.IsEditorHint())
        {
            FD.PushWarning($"{node} namely {node.GetPathTo(node.GetTree().GetEditedSceneRoot())} is trying to call GetChildrenCached in editor, which is not expected.");
            return null;
        }
    #endif
    
        if (node.HasData($"{ChildrenCacheTag}{tag}"))
        {
            return node.GetData<Array<T>>($"{ChildrenCacheTag}{tag}");
        }
        
        Array<T> result = new(node.GetChildren<T>(includeInternal));
        node.SetData($"{ChildrenCacheTag}{tag}", result);
        node.SetChildrenCacheMonitor(node, ChildrenCacheTag);
        return result;
    }

    public static IEnumerable<T> GetChildrenRecursivelyCached<[MustBeVariant] T>(this Node node,
        string tag = "Default", bool includeInternal = false) where T : Node
    {
    #if TOOLS
        if (Engine.IsEditorHint())
        {
            FD.PushWarning($"{node} namely {node.GetPathTo(node.GetTree().GetEditedSceneRoot())} is trying to call GetChildrenRecursivelyCached in editor, which is not expected.");
            return null;
        }
    #endif
    
        if (node.HasData($"{ChildrenRecursivelyCacheTag}{tag}"))
        {
            return node.GetData<Array<T>>($"{ChildrenRecursivelyCacheTag}{tag}");
        }
        
        Array<T> result = [];
        foreach (var child in node.GetChildrenRecursively(includeInternal))
        {
            if (child is T t) result.Add(t);
            node.SetChildrenCacheMonitor(child, ChildrenRecursivelyCacheTag);
        }
        node.SetData($"{ChildrenRecursivelyCacheTag}{tag}", result);
        node.SetChildrenCacheMonitor(node, ChildrenRecursivelyCacheTag);
        return result;
    }
    
    #endregion
    
    #region CanvasItem

    /// <summary>
    /// Useful to draw atlas texture with tiled mode
    /// </summary>
    public static void DrawTextureRectTiled(this CanvasItem item, Texture2D texture,
        Rect2 rect, Color? modulate = null)
    {
        var size = texture.GetSize();
        item.DrawTextureRectRegionTiled(texture, rect, 
            new(Vector2.Zero, size), modulate);
    }
    
    /// <summary>
    /// Useful to draw atlas texture with tiled mode
    /// </summary>
    public static void DrawTextureRectRegionTiled(this CanvasItem item, Texture2D texture,
        Rect2 rect, Rect2 srcRect, Color? modulate = null)
    {
        var flipH = rect.Size.X * srcRect.Size.X < 0f;
        var flipV = rect.Size.Y * srcRect.Size.Y < 0f;
        rect = new Rect2(rect.Position, rect.Size.Abs());
        srcRect = new Rect2(srcRect.Position, srcRect.Size.Abs());
        
        var rx = rect.Size.X;
        var ry = rect.Size.Y;
        var ux = srcRect.Size.X;
        var uy = srcRect.Size.Y;
        if (ux <= 0f || uy <= 0f) return;
        
        var px = 0f;
        while (px < rx)
        {
            var py = 0f;
            while (py < ry)
            {
                var w = Math.Min(ux, rx - px);
                var h = Math.Min(uy, ry - py);
                var x = flipH ? ux - w : 0f;
                var y = flipV ? uy - h : 0f;
                var sRect = new Rect2(x, y ,w ,h);
                if (flipH) w *= -1f;
                if (flipV) h *= -1f;
                var rRect = new Rect2(rect.Position + new Vector2(px, py), w, h);
                item.DrawTextureRectRegion(texture, rRect, sRect, modulate);
                py += uy;
            }
            px += ux;
        }
    }

    public static void SetShaderParam(this CanvasItem item, string param, Variant value)
    {
        if (item.Material is not ShaderMaterial shader) return;
        shader.SetShaderParameter(param, value);
    }

    public static T GetShaderParam<[MustBeVariant] T>(this CanvasItem item, string param)
    {
        if (item.Material is not ShaderMaterial shader) return default;
        return shader.GetShaderParameter(param) is T t ? t : default;
    }

    public static Texture2D TryGetTexture(this CanvasItem item)
    {
        if (item is Sprite2D spr) return spr.Texture;
        if (item is AnimatedSprite2D anim) return anim.SpriteFrames.GetFrameTexture(
            anim.Animation, anim.Frame);
        return null;
    }

    public static Tween FadeIn(this CanvasItem item, double time)
    {
        item.Modulate = item.Modulate with { A = 0f };
        var t = item.CreatePhysicsTween();
        t.TweenProperty(item, "modulate:a", 1f, time);
        return t;
    }
    
    #endregion
    
    #region TileMap

    public static bool HasLayer(this TileMap tilemap, string layer)
    {
        for (int i = 0; i < tilemap.GetLayersCount(); i++)
        {
            if (tilemap.GetLayerName(i) == layer) return true;
        }
        
        return false;
    }
    
    public static int GetLayerIndex(this TileMap tilemap, string layer)
    {
        for (int i = 0; i < tilemap.GetLayersCount(); i++)
        {
            if (tilemap.GetLayerName(i) == layer) return i;
        }
        
        return -1;
    }

    #endregion
    
    #region PhysicsBody
    
    public static bool IsOverlapping(this PhysicsBody2D body, Vector2 offset = default)
        => body.TestMove(
            body.GlobalTransform with { Origin = body.GlobalPosition + offset },
            Vector2.Zero
        );
        
    public static bool IsOverlapping(this PhysicsBody3D body, Vector3 offset = default)
        => body.TestMove(
            body.GlobalTransform with { Origin = body.GlobalPosition + offset },
            Vector3.Zero
        );
    
    #endregion
}