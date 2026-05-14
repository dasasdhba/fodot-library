using System;

namespace Moon.Component;

/// <summary>
/// manually track the value change
/// </summary>
public class Tracker<T>(T defaultValue) where T : IEquatable<T>
{
    public T Value { get; set; } = defaultValue;
    public double Time { get ;set; }
    
    /// <summary>
    /// Return true if the value changed
    /// </summary>
    public bool Update(T newValue, double delta)
    {
        Time += delta;
        if (!Value.Equals(newValue))
        {
            Value = newValue;
            Time = 0d;
            return true;
        }
        
        return false;
    }

    public void Reset(T value)
    {
        Value = value;
        Time = 0d;
    }
}