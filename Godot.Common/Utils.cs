using System;
using System.Collections.Generic;

namespace Godot;

public static class Utils
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
}