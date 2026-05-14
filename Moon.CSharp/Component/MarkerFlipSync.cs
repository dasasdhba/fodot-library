using Godot;
using Moon.Class;
using Moon.Utils;

namespace Moon.Component;

[GlobalClass]
public partial class MarkerFlipSync : Marker2D
{
    [ExportCategory("MarkerFlipSync")]
    [Export]
    public Node2D AnimNode {  get; set; }
    
    public enum MarkerFlipSyncProcessCallback { Idle, Physics }
    
    [Export]
    public MarkerFlipSyncProcessCallback ProcessCallback { get; set; } 
        = MarkerFlipSyncProcessCallback.Physics;
    
    public bool FlipH { get; set; } = false;
    public bool FlipV { get; set; } = false;
    public Vector2 Origin { get; set; }
    
    public override void _Ready()
    {
        Origin = Position;
        
        if (AnimNode != null) return;
        var p = GetParent();
        if (p is Sprite2D spr)
            AnimNode = spr;
        else if (p is AnimatedSprite2D anim)
            AnimNode = anim;
        else if (p is AnimGroup2D group)
            AnimNode = group;
        
        Update();
        
        this.AddProcess(Update, ProcessCallback == MarkerFlipSyncProcessCallback.Physics);
    }

    public void Update()
    {
        if (AnimNode is Sprite2D spr)
        {
            FlipH = spr.FlipH;
            FlipV = spr.FlipV;
        }
        else if (AnimNode is AnimatedSprite2D anim)
        {
            FlipH = anim.FlipH;
            FlipV = anim.FlipV;
        }
        else if (AnimNode is AnimGroup2D group)
        {
            FlipH = group.FlipH;
            FlipV = group.FlipV;
        }

        var pos = Origin;
        if (FlipH) pos.X *= -1f;
        if (FlipV) pos.Y *= -1f;

        Position = pos;
    }
}