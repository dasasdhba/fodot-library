using Godot;

namespace Moon.Class;

[GlobalClass, Tool]
#if TOOLS
public partial class TextureFill : NodeSize2D, ISerializationListener
#else
public partial class TextureFill : NodeSize2D
#endif
{
    [ExportCategory("TextureFill")]
    [Export]
    public Texture2D Texture
    {
        get => _texture;
        set
        {
            _texture = value;
            QueueRedraw();
        }
    }
    private Texture2D _texture;

    [Export]
    public bool FlipH
    {
        get => _flipH;
        set
        {
            _flipH = value;
            QueueRedraw();
        }    
    }
    private bool _flipH;

    [Export]
    public bool FlipV
    {
        get => _flipV;
        set
        {
            _flipV = value;
            QueueRedraw();
        }
    }
    private bool _flipV;

    public override void _Draw()
    {
        if (Texture == null) return;
        
        var size = Size;
        if (FlipH) size.X *= -1f;
        if (FlipV) size.Y *= -1f;
        
        DrawTextureRect(Texture, new(Vector2.Zero, size), true);
    }

    public TextureFill() : base()
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