using Godot;
using Godot.Collections;

namespace Moon.Class;

[GlobalClass, Tool]
public partial class View2DSetting : Node
{
    [ExportCategory("View2DSetting")]
    [Export]
    public bool AutoSetup { get; set; } = false;
    
    [Export]
    public bool ForceUpdate { get ;set; } = false;

    [Export]
    public bool RegionOverride
    {
        get => _RegionOverride;
        set
        {
            _RegionOverride = value;
            NotifyPropertyListChanged();
        }
    }

    private bool _RegionOverride = true;

    [Export]
    public ViewRect2D RegionRect { get ; set; }

    [Export]
    public bool RegionSmoothed
    {
        get => _RegionSmoothed;
        set
        {
            _RegionSmoothed = value;
            NotifyPropertyListChanged();
        }
    }
    private bool _RegionSmoothed = true;

    [Export]
    public double RegionSmoothTime { get; set; } = 0.5d;

    [Export]
    public bool FollowOverride
    {
        get => _FollowOverride;
        set
        {
            _FollowOverride = value;
            NotifyPropertyListChanged();
        }
    }

    private bool _FollowOverride = true;

    [Export]
    public CanvasItem FollowNode { get ;set; }

    [ExportGroup("Transform")]
    [Export]
    public bool MarginOverride
    {
        get => _MarginOverride;
        set
        {
            _MarginOverride = value;
            NotifyPropertyListChanged();
        }
    }
    private bool _MarginOverride = false;

    [Export]
    public Rect2 Margin { get ;set; } = new(Vector2.Zero, Vector2.Zero);

    [Export]
    public bool OffsetOverride
    {
        get => _OffsetOverride;
        set
        {
            _OffsetOverride = value;
            NotifyPropertyListChanged();
        }
    }
    private bool _OffsetOverride = false;

    [Export]
    public Vector2 Offset { get ;set;} = Vector2.Zero;

    [Export]
    public bool ZoomOverride
    {
        get => _ZoomOverride;
        set
        {
            _ZoomOverride = value;
            NotifyPropertyListChanged();
        }
    }
    private bool _ZoomOverride = false;

    [Export]
    public float Zoom { get ;set; } = 1f;

    [Export]
    public float MinZoom { get ;set; } = 1f;
    
    public View2DSetting() : base()
    {
    #if TOOLS
        if (Engine.IsEditorHint()) return;
    #endif    
    
        Ready += () =>
        {
            if (AutoSetup)
            {
                ApplySetting();
            }
        };
    }

#if TOOLS
    public override void _ValidateProperty(Dictionary property)
    {
        if (
            ((string)property["name"] == "RegionRect" && !RegionOverride) ||
            ((string)property["name"] == "RegionSmoothed" && !RegionOverride) ||
            ((string)property["name"] == "RegionSmoothTime" && !RegionOverride) ||
            ((string)property["name"] == "RegionSmoothTime" && !RegionSmoothed) ||
            ((string)property["name"] == "FollowNode" && !FollowOverride) ||
            ((string)property["name"] == "Margin" && !MarginOverride) ||
            ((string)property["name"] == "Offset" && !OffsetOverride) ||
            ((string)property["name"] == "Zoom" && !ZoomOverride) ||
            ((string)property["name"] == "MinZoom" && !ZoomOverride)
            )
        {
            property["usage"] = (uint)PropertyUsageFlags.ReadOnly;
        }
    }
#endif    

    public void ApplySetting()
    {
#if TOOLS
        if (Engine.IsEditorHint()) return;
#endif
    
        var view = this.GetView2D();
        if (!IsInstanceValid(view)) return;

        if (RegionOverride)
        {
            var rect = RegionRect?.GetViewRect() ?? new Rect2(Vector2.Zero, Vector2.Zero);
            view.ChangeRegion(rect, RegionSmoothed ? RegionSmoothTime : 0d);
        }
        if (FollowOverride)
        {
            view.FollowNode = FollowNode;
        }

        if (MarginOverride)
        {
            view.Margin = Margin;
        }

        if (OffsetOverride)
        {
            view.Offset = Offset;
        }

        if (ZoomOverride)
        {
            view.Zoom = Zoom;
            view.MinZoom = MinZoom;
        }

        if (ForceUpdate) view.ForceUpdate();
    }
}