using System;

namespace Godot.Warning;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class NotNullAttribute : Attribute
{
}
