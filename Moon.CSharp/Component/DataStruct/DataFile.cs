using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Globalization;
using Godot;
using Microsoft.VisualBasic;

namespace Moon.Component;

/// <summary>
/// Data file that stores a list or a dictionary
/// </summary>
public class DataFile
{
    private List<string[]> Data = [];

    public Error Load(string path, string splitter = ",")
    {
        using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
        if (file == null) return Error.Failed;
        
        Data.Clear();
        while (!file.EofReached())
        {
            var r = file.GetLine();
            if (r == "") continue;
            var result = r.Split(splitter);
            Data.Add(result);
        }
        
        file.Close();
        return Error.Ok;
    }

    public void LoadFromList<T>(IList<T> list, Func<T, string> parser)
    {
        Data.Clear();
        foreach (var i in list)
        {
            Data.Add([parser(i)]);
        }
    }
    
    public void LoadFromList(IList<string> list)
        => LoadFromList(list, i => i);
    public void LoadFromList(IList<int> list)
        => LoadFromList(list, i => i.ToString());
    public void LoadFromList(IList<float> list)
        => LoadFromList(list, i => i.ToString(CultureInfo.InvariantCulture));
    public void LoadFromList(IList<double> list)
        => LoadFromList(list, i => i.ToString(CultureInfo.InvariantCulture));

    public void LoadFromMap<T>(IDictionary<string, T> map, Func<T, string> parser)
    {
        Data.Clear();
        foreach (var (k, v) in map)
        {
            Data.Add([k, parser(v)]);
        }
    }
    
    public void LoadFromMap(IDictionary<string, string> map)
        => LoadFromMap(map, i => i);
    public void LoadFromMap(IDictionary<string, int> map)
        => LoadFromMap(map, i => i.ToString());
    public void LoadFromMap(IDictionary<string, float> map)
        => LoadFromMap(map, i => i.ToString(CultureInfo.InvariantCulture));
    public void LoadFromMap(IDictionary<string, double> map)
        => LoadFromMap(map, i => i.ToString(CultureInfo.InvariantCulture));
    
    public Error Save(string path, string splitter = ",")
    {
        using var file = FileAccess.Open(path, FileAccess.ModeFlags.Write);
        if (file == null) return Error.Failed;

        foreach (var r in Data)
        {
            file.StoreCsvLine(r, splitter);
        }
        
        file.Close();
        return Error.Ok;
    }
    
    public void Clear() => Data.Clear();

    public Dictionary<string, T> ToMap<T>(Func<string[], T> parser)
    {
        var map = new Dictionary<string, T>();
        foreach (var row in Data)
        {
            var key = Strings.Trim(row[0]);
            var value = parser(row[1..]);
            map[key] = value;
        }
        return map;
    }
    
    public FrozenDictionary<string, T> ToFrozenMap<T>(Func<string[], T> parser)
       => ToMap(parser).ToFrozenDictionary();
    
    public Dictionary<string, string> ToMap()
       => ToMap(rows => Strings.Trim(string.Join(null, rows)));
       
    public FrozenDictionary<string, string> ToFrozenMap()
       => ToMap().ToFrozenDictionary();
       
    public T[] ToArray<T>(Func<string[], T> parser)
    {
        var list = new List<T>();
        foreach (var row in Data)
        {
            var value = parser(row);
            list.Add(value);
        }
        return list.ToArray();
    }
    
    public string[] ToArray()
        => ToArray(rows => Strings.Trim(string.Join(null, rows)));
    
    public int[] ToIntArray()
        => ToArray(rows => int.Parse(Strings.Trim(string.Join(null, rows))));
    
    public float[] ToFloatArray()
        => ToArray(rows => float.Parse(Strings.Trim(string.Join(null, rows))));
    
    public double[] ToDoubleArray()
        => ToArray(rows => double.Parse(Strings.Trim(string.Join(null, rows))));
}