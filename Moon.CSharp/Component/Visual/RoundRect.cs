using System;
using System.Collections.Generic;
using Godot;
using Moon.Class;

namespace Moon.Component;

[GlobalClass, Tool]
#if TOOLS
public partial class RoundRect : NodeSize2D, ISerializationListener
#else
public partial class RoundRect : NodeSize2D
#endif
{
    [ExportCategory("RoundRect")]
    [Export]
    public Color Color
    {
        get  => _Color;
        set
        {
            _Color = value;
            QueueRedraw();
        }    
    }
    private Color _Color = Colors.Black;
    
    [Export]
    public float Radius
    {
        get  => _Radius;
        set
        {
            _Radius = value;
            QueueRedraw();
        }
    }
    
    private float _Radius = 16f;

    [Export]
    public int RoundPoint
    {
        get  => _RoundPoint;
        set
        {
            _RoundPoint = value;
            QueueRedraw();
        }
    }
    
    private int _RoundPoint = 16;

    public RoundRect() : base()
    {
        Ready += QueueRedraw;
        SignalSizeChanged += QueueRedraw;
    }
    
#if TOOLS
    
    public void OnBeforeSerialize()
    {
        Ready -= QueueRedraw;
        SignalSizeChanged -= QueueRedraw;
    }

    public void OnAfterDeserialize()
    {
        QueueRedraw();
    }
    
#endif

    private IEnumerable<Vector2> GetRoundedPoints(Vector2 origin, Vector2 radius, float from, float to)
    {
        for (int i = 0; i < RoundPoint; i++)
        {
            float angle = Mathf.Lerp(from, to, (float)i / RoundPoint);
            yield return origin + radius * Vector2.Right.Rotated(angle);
        }
    }

    public override void _Draw()
    {
        var rx = Math.Min(Radius, Size.X / 2f);
        var ry = Math.Min(Radius, Size.Y / 2f);
        var radius = new Vector2(rx, ry);
        List<Vector2> points = [new(rx, 0f), new(Size.X - rx, 0f)];
        points.AddRange(GetRoundedPoints(
            new Vector2(Size.X - rx, ry), radius,
             -float.Pi / 2f, 0f));
        points.Add(new(Size.X, ry));
        points.Add(new(Size.X, Size.Y - ry));
        points.AddRange(GetRoundedPoints(
            new Vector2(Size.X - rx, Size.Y - ry), radius,
                 0f, float.Pi / 2f));
        points.Add(new(Size.X - rx, Size.Y));
        points.Add(new(rx, Size.Y));
        points.AddRange(GetRoundedPoints(
            new Vector2(rx, Size.Y - ry), radius,
                 float.Pi / 2f, float.Pi));
        points.Add(new(0f, Size.Y - ry));
        points.Add(new(0f, ry));
        points.AddRange(GetRoundedPoints(
            new Vector2(rx, ry), radius,
                 float.Pi, 3f * float.Pi / 2f));
        
        DrawColoredPolygon(points.ToArray(), Color);         
    }
}