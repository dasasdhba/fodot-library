using System;
using System.Collections.Generic;
using Godot;

namespace Moon;

public static class GodotExtensions
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

    public static Dictionary<string, Variant> GetSection(this ConfigFile config, string section)
    {
        var result = new Dictionary<string, Variant>();
        if (!config.HasSection(section)) return result;
        
        foreach (var key in config.GetSectionKeys(section))
        {
            result[key] = config.GetValue(section, key);
        }
        return result;
    }

    public static void SetSection(this ConfigFile config, string section, Dictionary<string, Variant> values)
    {
        foreach (var key in values.Keys)
        {
            config.SetValue(section, key, values[key]);
        }
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
    
    #endregion
}