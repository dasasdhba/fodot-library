using System;

#nullable enable

namespace Moon;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class GDMemberAttribute : Attribute
{
    public GDMemberAttribute()
    {
    }

    public GDMemberAttribute(string name)
    {
        Name = name;
    }

    public string? Name { get; }
}
