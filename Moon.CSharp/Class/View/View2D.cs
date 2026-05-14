using System;
using Godot;
using Moon.Utils;

namespace Moon.Class;

/// <summary>
/// Alternate of Godot's Camera
/// </summary>
[GlobalClass]
public partial class View2D : Node2D
{
    public enum View2DProcessCallbackMode { Idle, Physics }

    [ExportCategory("View2D")]
    [Export]
    public View2DProcessCallbackMode ProcessCallbackMode { get ;set; } = View2DProcessCallbackMode.Physics;
    
    [Export]
    public Rect2 Region
    {
        get => _Region;
        private set
        {
            _Region = value;
            ChangingRegion = false;
        }
    }
    private Rect2 _Region = new(Vector2.Zero, new(512f, 288f));
    
    public Rect2 GetRegion() => Region;
    
    [ExportGroup("Transform")]
    [Export]
    public Rect2 Margin { get ;set; } = new(Vector2.Zero, Vector2.Zero);

    [Export]
    public Vector2 Offset { get ;set;} = Vector2.Zero;

    [Export]
    public float Zoom { get ;set; } = 1f;

    [Export]
    public float MinZoom { get ;set; } = 1f;

    [Export]
    public float Rot { get ; set; } = 0f;
    
    [Export]
    public Vector2 TransformOffset { get ;set;} = Vector2.Zero;

    [ExportGroup("Smooth")]
    [Export]
    public bool SmoothEnabled { get ;set; }  = true;

    [Export]
    public float SmoothRate { get ;set; } = 20f;

    [Export]
    public bool SmoothZoomEnabled { get ;set; } = true;

    [Export]
    public float SmoothZoomRate { get ;set; } = 10f;

    [Export]
    public bool SmoothRotEnabled { get ;set; } = true;

    [Export]
    public float SmoothRotRate { get ;set; } = 10f;

    [ExportGroup("Shake")]
    [Export]
    public bool ShakeOutRegion { get ;set; } = false;
    
    [Export]
    public double ShakeInterval { get ;set; } = 0.03d;

    [Export]
    public Vector2 ShakeAmp { get ;set; } = new(12f, 12f);

    [ExportGroup("Follow")]
    [Export]
    public CanvasItem FollowNode { get ; set; }

    [Export]
    public Vector2 FollowOffset { get ; set; } = Vector2.Zero;
    
    [Signal]
    public delegate void RegionChangeStartedEventHandler();
    
    [Signal]
    public delegate void RegionChangeFinishedEventHandler();

    private Vector2 CenterPosition { get ;set; }
    public Vector2 GetCenterPosition() => CenterPosition;

    private Vector2 ShakeOffset { get ;set; } = Vector2.Zero;
    private double ShakeTimer { get ;set; } = 0d;
    private double ShakeIntervalTimer { get ;set; } = 0d;

    public void ShakeStart(double time = 0.1d) => ShakeTimer = Math.Max(ShakeTimer, time);

    public void ShakeStop()
    {
        ShakeTimer = 0d;
        ShakeIntervalTimer = 0d;
        ShakeOffset = Vector2.Zero;
    }
    
    public Vector2 GetTargetPosition() 
        => GetTargetPosition(GlobalPosition + Offset);
    
    public Vector2 GetTargetPosition(Vector2 target)
    {
        var result = CenterPosition;

        if (Margin.End.X == 0f)
        {
            if (target.X > result.X)
                result.X = target.X;
        }
        else if (Margin.End.X > 0f && target.X > result.X + Margin.End.X)
            result.X = target.X - Margin.End.X;

        if (Margin.Position.X == 0f)
        {
            if (target.X < result.X)
                result.X = target.X;
        }
        else if (Margin.Position.X < 0f && target.X < result.X + Margin.Position.X)
            result.X = target.X - Margin.Position.X;

        if (Margin.End.Y == 0f)
        {
            if (target.Y > result.Y)
                result.Y = target.Y;
        }
        else if (Margin.End.Y > 0f && target.Y > result.Y + Margin.End.Y)
            result.Y = target.Y - Margin.End.Y;

        if (Margin.Position.Y == 0f)
        {
            if (target.Y < result.Y)
                result.Y = target.Y;
        }
        else if (Margin.Position.Y < 0f && target.Y < result.Y + Margin.Position.Y)
            result.Y = target.Y - Margin.Position.Y;

        return result;
    }

    private float CurrentZoom { get ;set; }
    public float GetCurrentZoom() => CurrentZoom;

    private float CurrentRot { get ;set; }
    public float GetCurrentRot() => CurrentRot;

    public View2D() : base()
    {
        TreeEntered += () =>
        {
            var viewport = GetViewport();
            viewport.SetMeta("ViewportView2D", this);
        };

        Ready += () =>
        {
            ForceUpdate();
            this.AddProcess(ViewProcess, ProcessCallbackMode == View2DProcessCallbackMode.Physics);
        };
    }

    public void ForceUpdate()
    {
        if (IsInstanceValid(FollowNode))
        {
            GlobalPosition = (Vector2)FollowNode.Get(Node2D.PropertyName.GlobalPosition) + FollowOffset;
        }

        CurrentZoom = GetLimitZoom(Zoom);
        CurrentRot = Rot;
        CenterPosition = GetLimitPosition(GetTargetPosition());
        SetView(CenterPosition, CurrentZoom, CurrentRot);
    }

    private bool ChangingRegion { get ;set; } = false;
    private Vector2 ChangingOrigin { get; set; }
    private float ChangingZoom { get ; set; }
    private double ChangingTime { get ; set; }
    private double ChangingTimer { get ; set; }
    private Func<double, double> ChangingInterp { get ;set; }

    public bool IsChangingRegion() => ChangingRegion;

    public void ChangeRegion(Rect2 region, double time = -1d)
        => ChangeRegion(region, time, t => (3f - 2f * t) * t * t);

    public void ChangeRegion(Rect2 region, double time, Func<double, double> interp)
    {
        if (Region == region) return;
        
        Region = region;
        EmitSignalRegionChangeStarted();
    
        if (time <= 0d)
        {
            if (time < 0d) ForceUpdate();
            EmitSignalRegionChangeFinished();
            return;
        }

        ChangingRegion = true;
        ChangingOrigin = CenterPosition;
        ChangingZoom = CurrentZoom;
        ChangingTime = time;
        ChangingTimer = 0d;
        ChangingInterp = interp;
    }

    protected virtual void ViewProcess(double delta)
    {
        // follow

        if (IsInstanceValid(FollowNode))
        {
            GlobalPosition = (Vector2)FollowNode.Get(Node2D.PropertyName.GlobalPosition) + FollowOffset;
        }

        // shake

        if (ShakeTimer > 0d)
        {
            ShakeTimer -= delta;
            ShakeIntervalTimer += delta;
            if (ShakeIntervalTimer >= ShakeInterval)
            {
                ShakeIntervalTimer -= ShakeInterval;
                ShakeOffset = new Vector2(
                    Mathe.RandfRange(-ShakeAmp.X, ShakeAmp.X),
                    Mathe.RandfRange(-ShakeAmp.Y, ShakeAmp.Y)
                );
            }
            if (ShakeTimer <= 0d) ShakeStop();
        }

        // view

        var target = GetTargetPosition();
        target = ShakeOutRegion ? GetLimitPosition(target) + ShakeOffset :
            GetLimitPosition(target + ShakeOffset);
        var zoom = GetLimitZoom(Zoom);
        var rot = Rot;

        if (ChangingRegion)
        {
            ChangingTimer += delta;
            var t = Math.Min(1d, ChangingTimer / ChangingTime);
            var p = (float)ChangingInterp.Invoke(t);
            CenterPosition = (1f - p) * ChangingOrigin + p * target;
            CurrentZoom = (1f - p) * ChangingZoom + p * zoom;
            if (ChangingTimer >= ChangingTime)
            {
                ChangingRegion = false;
                EmitSignalRegionChangeFinished();
            }
        }
        else
        {
            if (SmoothEnabled)
                CenterPosition = CenterPosition.MoveToward(
                    target,
                    (target - CenterPosition).Length() * (float)(delta * SmoothRate)
                    );
            else
                CenterPosition = target;

            if (SmoothZoomEnabled)
                CurrentZoom = Mathf.MoveToward(
                    CurrentZoom,
                    zoom,
                    Math.Abs(zoom - CurrentZoom) * (float)(delta * SmoothZoomRate)
                    );
            else
                CurrentZoom = zoom;
        }

        if (SmoothRotEnabled)
            CurrentRot = Mathe.MoveTowardAngle(
                CurrentRot,
                rot,
                Math.Abs(rot - CurrentRot) * (float)(delta * SmoothRotRate)
                );
        else
            CurrentRot = rot;

        SetView(CenterPosition, CurrentZoom, CurrentRot);
    }

    public Vector2 GetLimitPosition(Rect2 region, float zoom, Vector2 position)
    {
        var size = GetViewportRect().Size / zoom;
        var xUnlimit = region.Size.X <= 0f;
        var yUnlimit = region.Size.Y <= 0f;
        
        return new(
            xUnlimit ? position.X : region.Size.X < size.X ? position.X : Math.Max(region.Position.X + size.X / 2f, Math.Min(region.End.X - size.X / 2f, position.X)),
            yUnlimit ? position.Y : region.Size.Y < size.Y ? position.Y : Math.Max(region.Position.Y + size.Y / 2f, Math.Min(region.End.Y - size.Y / 2f, position.Y))
            );
    }

    public Vector2 GetLimitPosition(Vector2 position) => GetLimitPosition(Region, CurrentZoom, position);

    public float GetLimitZoom(Rect2 region, float zoom)
    {
        var size = GetViewportRect().Size;
        var zx = size.X / region.Size.X;
        var zy = size.Y / region.Size.Y;
        return Mathe.Max(zy, zx, zoom, MinZoom);
    }

    public float GetLimitZoom(float zoom) => GetLimitZoom(Region, zoom);

    public void SetView(Vector2 position, float zoom, float rotation)
    {
        var view = GetViewport();
        var size = GetViewportRect().Size;
        var transform = new Transform2D(rotation, zoom * Vector2.One, 0f, TransformOffset);
        var pos = transform * position;
        var origin = -pos + size / 2f;
        
        // this will fix parallax2d's jitter issue with snap 2d transform to pixel enabled
        // but it will cause camera jitter slightly
        
        /*if (view.IsSnap2DTransformsToPixelEnabled())
            origin = (origin + 0.5f * Vector2.One).Floor();*/
        
        transform = transform with { Origin = origin };
        view.CanvasTransform = transform;
        
        // update parallax objects
        
        GetTree().CallGroup("__cameras_" + view.GetViewportRid().Id,
             "_camera_moved", transform, size / 2f, -origin);
             
        // update view rect
        
        var topLeft = -transform.Origin / transform.Scale;
        var viewSize = size / transform.Scale;
        CurrentViewRect = new(topLeft, viewSize);
    }
    
    private Rect2 CurrentViewRect;
    public Rect2 GetCurrentViewRect() => CurrentViewRect;
}