using Godot;

namespace Moon.Class;

[GlobalClass, Tool]
#if TOOLS
public partial class ColorFill : NodeSize2D, ISerializationListener
#else
public partial class ColorFill : NodeSize2D
#endif
{
    [ExportCategory("ColorFill")]
    [Export]
    public Color Color
    {
        get => _Color;
        set
        {
            _Color = value;
            QueueRedraw();
        }
    }
    
    private Color _Color = new(0f, 0f, 0f, 1f);

    public override void _Draw()
    {
        DrawRect(new (new(0f, 0f), Size), Color);
    }

    public ColorFill() : base()
    {
        TreeEntered += QueueRedraw;
        SignalSizeChanged += QueueRedraw;
    }
    
#if TOOLS
    
    public void OnBeforeSerialize()
    {
        TreeEntered -= QueueRedraw;
        SignalSizeChanged -= QueueRedraw;
    }

    public void OnAfterDeserialize()
    {
        QueueRedraw();
    }

#endif
}