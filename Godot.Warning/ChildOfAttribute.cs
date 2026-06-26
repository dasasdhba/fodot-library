using System;

namespace Godot.Warning;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class ChildOfAttribute : Attribute
{
    public ChildOfAttribute(string parentType)
    {
        ParentType = parentType;
    }

    public string ParentType { get; }
}
