using Godot;
using Godot.Collections;

namespace Moon.Class;

[GlobalClass, Tool]
public partial class NodeSize2D : Node2D
{
    [ExportCategory("NodeSize2D")]
    [Export]
    public Vector2 Size
    {
        get => _Size;
        set
        {
            if (_Size != value)
            {
                _Size = value;
                EmitSignal(SignalName.SizeChanged);
            }
        }
    }
    
    private Vector2 _Size = new(32f, 32f);
    
    [Signal]
    public delegate void SizeChangedEventHandler();

#if TOOLS    
    public override void _EditorGetState(Dictionary state)
        => state["size"] = Size;

    public override void _EditorSetState(Dictionary state)
        => Size = (Vector2)state["size"];

    public override bool _EditorUseRect()
        => true;

    public override Rect2 _EditorGetRect()
        => new Rect2(Vector2.Zero, Size).Abs();

    public override void _EditorSetRect(Rect2 rect)
    {
        Position += Transform.BasisXform(rect.Position).Snapped(1f);
        Size = rect.Size.Abs().Snapped(1f);
    }
    
    public override bool _EditorUsePivot()
        => true;

    public override Vector2 _EditorGetPivot()
        => Vector2.Zero;

    public override void _EditorSetPivot(Vector2 pivot)
        => Position = Transform * pivot;
#endif

}