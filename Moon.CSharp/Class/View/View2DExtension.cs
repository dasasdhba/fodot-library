using System;
using Godot;

namespace Moon.Class;

/// <summary>
/// Useful function to get current view info.
/// </summary>
public static class View2DExtension
{
    public static void ViewShake(this Node node, double time = 0.1d)
        => node.GetView2D().ShakeStart(time);

    public static void ViewShakeStop(this Node node)
        => node.GetView2D().ShakeStop();

    public static View2D GetView2D(this Node node)
    {
        if (node.HasMeta("ViewportView2D"))
            return node.GetMeta("ViewportView2D").As<View2D>();
        var v = node.GetViewport();
        return v.HasMeta("ViewportView2D") ? 
            v.GetMeta("ViewportView2D").As<View2D>() : null;
    }

    /// <summary>
    /// Return the current view rect.
    /// </summary>
    public static Rect2 GetViewRect(this Node node, bool forceUpdate = false)
    {
        if (!forceUpdate)
        {
            var view = node.GetView2D();
            if (view is not null) return view.GetCurrentViewRect();
        }

        if (node is CanvasItem item)
        {
            var canvas = item.GetCanvasTransform();
            var topLeft = -canvas.Origin / canvas.Scale;
            var size = item.GetViewportRect().Size / canvas.Scale;
                    
            return new(topLeft, size);
        }

        return default;
    }

    /// <summary>
    /// Whether the CanvasItem is in current view
    /// </summary>
    /// <param name="item">The CanvasItem to query.</param>
    /// <param name="eps">Set positive to extend judging view, or negative to reduce.</param>
    /// <param name="forceUpdate">Whether to use buffered view if available.</param>
    public static bool IsInView(this CanvasItem item, float eps = 0f, bool forceUpdate = false)
    {
        var pos = (Vector2)item.Get(Node2D.PropertyName.GlobalPosition);
        return GetViewRect(item, forceUpdate).Grow(eps).HasPoint(pos);
    }

    /// <summary>
    /// Whether the CanvasItem is in current view left
    /// </summary>
    /// <param name="item">The CanvasItem to query.</param>
    /// <param name="eps">Set positive to extend judging view, or negative to reduce.</param>
    /// <param name="forceUpdate">Whether to use buffered view if available.</param>
    public static bool IsInViewLeft(this CanvasItem item, float eps = 0f, bool forceUpdate = false)
    {
        var pos = (Vector2)item.Get(Node2D.PropertyName.GlobalPosition);
        return GetViewRect(item, forceUpdate).Position.X - eps <= pos.X;
    }

    /// <summary>
    /// Whether the CanvasItem is in current view right
    /// </summary>
    /// <param name="item">The CanvasItem to query.</param>
    /// <param name="eps">Set positive to extend judging view, or negative to reduce.</param>
    /// <param name="forceUpdate">Whether to use buffered view if available.</param>
    public static bool IsInViewRight(this CanvasItem item, float eps = 0f, bool forceUpdate = false)
    {
        var pos = (Vector2)item.Get(Node2D.PropertyName.GlobalPosition);
        return GetViewRect(item, forceUpdate).End.X + eps >= pos.X;
    }

    /// <summary>
    /// Whether the CanvasItem is in current view top
    /// </summary>
    /// <param name="item">The CanvasItem to query.</param>
    /// <param name="eps">Set positive to extend judging view, or negative to reduce.</param>
    /// <param name="forceUpdate">Whether to use buffered view if available.</param>
    public static bool IsInViewTop(this CanvasItem item, float eps = 0f, bool forceUpdate = false)
    {
        var pos = (Vector2)item.Get(Node2D.PropertyName.GlobalPosition);
        return GetViewRect(item, forceUpdate).Position.Y - eps <= pos.Y;
    }

    /// <summary>
    /// Whether the CanvasItem is in current view bottom
    /// </summary>
    /// <param name="item">The CanvasItem to query.</param>
    /// <param name="eps">Set positive to extend judging view, or negative to reduce.</param>
    /// <param name="forceUpdate">Whether to use buffered view if available.</param>
    public static bool IsInViewBottom(this CanvasItem item, float eps = 0f, bool forceUpdate = false)
    {
        var pos = (Vector2)item.Get(Node2D.PropertyName.GlobalPosition);
        return GetViewRect(item, forceUpdate).End.Y + eps >= pos.Y;
    }

    /// <summary>
    /// Whether the CanvasItem is in current view with specific direction.
    /// </summary>
    /// <param name="item">The CanvasItem to query.</param>
    /// <param name="dir">The direction to query.</param>
    /// <param name="eps">Set positive to extend judging view, or negative to reduce.</param>
    /// <param name="forceUpdate">Whether to use buffered view if available.</param>
    public static bool IsInViewDir(this CanvasItem item, Vector2 dir, float eps = 0f, bool forceUpdate = false)
    {
        if (Math.Abs(dir.Y) >= Math.Abs(dir.X))
        {
            return dir.Y >= 0 ? IsInViewBottom(item, eps, forceUpdate) : IsInViewTop(item, eps, forceUpdate);
        }

        return dir.X >= 0 ? IsInViewRight(item, eps, forceUpdate) : IsInViewLeft(item, eps, forceUpdate);
    }
    
    /// <summary>
    /// Return the current view region
    /// </summary>
    public static Rect2 GetViewRegion(this Node node)
    {
        var view = node.GetView2D();
        if (view is not null) return view.GetRegion();

        return default;
    }

    /// <summary>
    /// Whether the CanvasItem is in current view region
    /// </summary>
    /// <param name="item">The CanvasItem to query.</param>
    /// <param name="eps">Set positive to extend judging view, or negative to reduce.</param>
    public static bool IsInViewRegion(this CanvasItem item, float eps = 0f)
    {
        var pos = (Vector2)item.Get(Node2D.PropertyName.GlobalPosition);
        return GetViewRegion(item).Grow(eps).HasPoint(pos);
    }

    /// <summary>
    /// Whether the CanvasItem is in current view region left
    /// </summary>
    /// <param name="item">The CanvasItem to query.</param>
    /// <param name="eps">Set positive to extend judging view, or negative to reduce.</param>
    public static bool IsInViewRegionLeft(this CanvasItem item, float eps = 0f)
    {
        var pos = (Vector2)item.Get(Node2D.PropertyName.GlobalPosition);
        return GetViewRegion(item).Position.X - eps <= pos.X;
    }

    /// <summary>
    /// Whether the CanvasItem is in current view region right
    /// </summary>
    /// <param name="item">The CanvasItem to query.</param>
    /// <param name="eps">Set positive to extend judging view, or negative to reduce.</param>
    public static bool IsInViewRegionRight(this CanvasItem item, float eps = 0f)
    {
        var pos = (Vector2)item.Get(Node2D.PropertyName.GlobalPosition);
        return GetViewRegion(item).End.X + eps >= pos.X;
    }

    /// <summary>
    /// Whether the CanvasItem is in current view region top
    /// </summary>
    /// <param name="item">The CanvasItem to query.</param>
    /// <param name="eps">Set positive to extend judging view, or negative to reduce.</param>
    public static bool IsInViewRegionTop(this CanvasItem item, float eps = 0f)
    {
        var pos = (Vector2)item.Get(Node2D.PropertyName.GlobalPosition);
        return GetViewRegion(item).Position.Y - eps <= pos.Y;
    }

    /// <summary>
    /// Whether the CanvasItem is in current view region bottom
    /// </summary>
    /// <param name="item">The CanvasItem to query.</param>
    /// <param name="eps">Set positive to extend judging view, or negative to reduce.</param>
    public static bool IsInViewRegionBottom(this CanvasItem item, float eps = 0f)
    {
        var pos = (Vector2)item.Get(Node2D.PropertyName.GlobalPosition);
        return GetViewRegion(item).End.Y + eps >= pos.Y;
    }

    /// <summary>
    /// Whether the CanvasItem is in current view region with specific direction.
    /// </summary>
    /// <param name="item">The CanvasItem to query.</param>
    /// <param name="dir">The direction to query.</param>
    /// <param name="eps">Set positive to extend judging view, or negative to reduce.</param>
    public static bool IsInViewRegionDir(this CanvasItem item, Vector2 dir, float eps = 0f)
    {
        if (Math.Abs(dir.Y) >= Math.Abs(dir.X))
        {
            return dir.Y >= 0 ? IsInViewRegionBottom(item, eps) : IsInViewRegionTop(item, eps);
        }

        return dir.X >= 0 ? IsInViewRegionRight(item, eps) : IsInViewRegionLeft(item, eps);
    }
}