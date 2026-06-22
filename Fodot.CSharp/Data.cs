using System;
using Godot;

namespace Fodot.CSharp;

// better metadata for godot objects

public static class Data
{
    public static void SetData(this GodotObject obj, StringName tag, Variant value)
        => obj.SetMeta(tag, value);

    public static void SetData(this GodotObject obj, Rid rid, StringName tag, Variant value)
    {
        Extend.GodotObject.setData(tag, value, rid, obj);
    }
    
    public static bool HasData(this GodotObject obj, StringName tag)
    {
        return GodotObjectModule.hasMeta(tag, obj);
    }

    public static bool RemoveData(this GodotObject obj, StringName tag)
    {
        return GodotObjectModule.removeMeta(tag, obj);
    }
    
    public static bool HasCustomData(this TileSet tileset, string tag)
    {
        return Extend.TileSet.hasCustomData(tag, tileset);
    }
    
    public static bool RemoveCustomData(this TileSet tileset, string tag)
    {
        return Extend.TileSet.removeCustomData(tag, tileset);
    }
    
    /// <summary>
    /// If the obj is TileMap or TileMapLayer, this method checks custom layer data instead.
    /// To check the metadata, using HasData Instead.
    /// </summary>
    public static bool HasTilesetData(this GodotObject obj, StringName tag)
    {
        return Extend.GodotObject.hasData(tag, obj);
    }
    
    /// <summary>
    /// If the obj is TileMap or TileMapLayer, this method removes custom layer data instead.
    /// To remove the metadata, using RemoveData Instead.
    /// </summary>
    public static bool RemoveTilesetData(this GodotObject obj, StringName tag)
    {
        return Extend.GodotObject.removeData(tag, obj);
    }
    
    public static T GetData<[MustBeVariant] T>(this GodotObject obj, StringName tag, Func<T> defaultFunc)
    {
        return GodotObjectModule.getMetaWithDefaultAs(tag, defaultFunc.AsFSharpFunc(), obj);
    }
    
    public static T GetData<[MustBeVariant] T>(this GodotObject obj, Rid rid, StringName tag, Func<T> defaultFunc)
    {
        return Extend.GodotObject.getDataWithDefaultAs(tag, defaultFunc.AsFSharpFunc(), rid, obj);
    }

    public static T GetData<[MustBeVariant] T>(this GodotObject obj, StringName tag, T defaultValue = default)
    {
        return obj.GetData(tag, () => defaultValue);
    }
    
    public static T GetData<[MustBeVariant] T>(this GodotObject obj, Rid rid, StringName tag, T defaultValue = default)
    {
        return obj.GetData(rid, tag, () => defaultValue);
    }
}
