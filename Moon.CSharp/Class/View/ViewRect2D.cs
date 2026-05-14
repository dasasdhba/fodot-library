using Godot;

namespace Moon.Class;

[GlobalClass, Tool]
public partial class ViewRect2D : DebugFill
{
    public Rect2 GetViewRect() => new(GlobalPosition, Size);
}