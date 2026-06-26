using System;

namespace Godot.Warning;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class NotNullStringAttribute : Attribute
{
    public NotNullStringAttribute()
    {
    }

    public NotNullStringAttribute(string defaultValue)
    {
        DefaultValue = defaultValue;
    }

    public string? DefaultValue { get; }
}
