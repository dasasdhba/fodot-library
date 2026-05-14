using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;
using Moon.Utils;

namespace Moon.Component;

// we use RenderingServer for better performance
// DO NOT CHANGE Interval, ShadowTime and ShadowItems in runtime.

// HINT: keep emitting for a long time may cause overflow
// this is due to layer issue, and we don't like to do sort operation like Nodes
// (which is slow)

[GlobalClass]
public partial class ShadowCaster2D : Node
{
    [ExportCategory("ShadowCaster2D")]
    [Export]
    public bool Emitting { get; set; } = false;
    
    [Export(PropertyHint.Range, "0.001,4096,or_greater")]
    public double Interval { get; set; } = 0.06d;
    
    [Export(PropertyHint.Range, "0.001,4096,or_greater")]
    public double ShadowTime { get; set; } = 0.5d;
    
    /// <summary>
    /// Relative to ShadowItem's ZIndex.
    /// </summary>
    [Export]
    public int ZIndex { get; set; } = -3;
    
    [ExportGroup("ProcessSettings")]
    [Export]
    public bool ForceVisible { get; set; } = false;
    
    [Export]
    public bool DuplicateMaterial { get; set; } = false;
    
    public enum ShadowCaster2DProcessCallback { Idle, Physics }
    
    [Export]
    public ShadowCaster2DProcessCallback ProcessCallback { get; set; } 
        = ShadowCaster2DProcessCallback.Physics;
    
    /// <summary>
    /// Increase this if queue overflow happens.
    /// </summary>
    [Export]
    public uint BufferCount { get; set; } = 5;
    
    [ExportGroup("Dependency")]
    [Export]
    public CanvasItem Root { get; set; }
    
    /// <summary>
    /// Must be Sprite2D or AnimatedSprite2D.
    /// </summary>
    [Export]
    public Array<CanvasItem> ShadowItems { get; set; }

    private class ShadowItem
    {
        private Rid Id;
        
        private Node Parent;
        private Rid ParentId;
        private double Time;
        private int ZIndex;
        private bool DuplicateMaterial;
        private bool ForceVisible;
        
        private CanvasItem SyncItem;
        private Texture2D Texture;
        
        private double Timer;
        private Color Modulate;

        public ShadowItem(ShadowCaster2D caster, CanvasItem syncItem)
        {
            Parent = caster.Root.GetParent();
            ParentId = Parent is CanvasItem item ? item.GetCanvasItem() : default;
            
            if (Parent != null) Parent.TreeExited += Free;
            
            Time = caster.ShadowTime;
            ZIndex = caster.ZIndex;
            DuplicateMaterial = caster.DuplicateMaterial;
            ForceVisible = caster.ForceVisible;
            
            SyncItem = syncItem;
        
            Id = RenderingServer.CanvasItemCreate();
            RenderingServer.CanvasItemSetParent(Id, ParentId);
        }

        public bool Init(int index)
        {
            if (!ForceVisible && !SyncItem.IsVisibleInTree()) return false;
            
            Texture2D texture = null;
            bool centered = false;
            bool flipH = false;
            bool flipV = false;
            
            if (SyncItem is Sprite2D spr)
            {
                texture = spr.Texture;
                centered = spr.Centered;
                flipH = spr.FlipH;
                flipV = spr.FlipV;
            }
            else if (SyncItem is AnimatedSprite2D anim)
            {
                texture = anim.SpriteFrames.GetFrameTexture(anim.Animation, anim.Frame);
                centered = anim.Centered;
                flipH = anim.FlipH;
                flipV = anim.FlipV;
            }

            if (texture != Texture)
            {
                RenderingServer.CanvasItemClear(Id);
                
                Texture = texture;
                if (Texture == null) return false;
                
                var pos = centered ? 
                    -new Vector2(Texture.GetWidth(), Texture.GetHeight()) / 2f : 
                    Vector2.Zero;
                
                Texture.Draw(Id, pos);
            }
            
            var transform = SyncItem.GetGlobalTransform();
            if (Parent is CanvasItem canvas)
            {
                transform = canvas.GetGlobalTransform().AffineInverse() * transform;
            }
            var scale = transform.Scale;

            if (flipH) scale.X *= -1f;
            if (flipV) scale.Y *= -1f;
            
            transform = new(transform.Rotation, scale, transform.Skew, transform.Origin);
            
            var material = DuplicateMaterial ? 
                (Material)SyncItem.Material.Duplicate() : 
                SyncItem.Material;
            
            RenderingServer.CanvasItemSetDrawIndex(Id, index);
            RenderingServer.CanvasItemSetTransform(Id, transform);
            RenderingServer.CanvasItemSetZIndex(Id, SyncItem.ZIndex + ZIndex);
            RenderingServer.CanvasItemSetModulate(Id, SyncItem.Modulate);
            RenderingServer.CanvasItemSetSelfModulate(Id, SyncItem.SelfModulate);
            RenderingServer.CanvasItemSetMaterial(Id, material?.GetRid() ?? new Rid());
            RenderingServer.CanvasItemSetVisible(Id, true);
            
            Timer = 0d;
            Modulate = SyncItem.Modulate;
            
            return true;
        }
        
        public bool Update(double delta)
        {
            Timer += delta;
            
            var alpha = Math.Max(0d, (1d - Timer / Time) * Modulate.A);
            RenderingServer.CanvasItemSetModulate(Id, Modulate with { A = (float)alpha });
            
            var result = Timer >= Time;

            if (result)
            {
                RenderingServer.CanvasItemSetVisible(Id, false);
            }
            
            return result;
        }
        
        public void Free() => RenderingServer.FreeRid(Id);
    }

    private partial class ShadowNode : Node
    {
        private Queue<ShadowItem>[] ShadowQueues;
        private Queue<ShadowItem>[] WorkingQueues;

        public ShadowNode(ShadowCaster2D caster) : base()
        {
            var shadowCount = GetShadowCount(caster.Interval, caster.ShadowTime)
                + caster.BufferCount;
            var itemCount = caster.ShadowItems.Count;
            
            ShadowQueues = new Queue<ShadowItem>[itemCount];
            WorkingQueues = new Queue<ShadowItem>[itemCount];
            
            for (int i = 0; i < itemCount; i++)
            {
                ShadowQueues[i] = new Queue<ShadowItem>();
                WorkingQueues[i] = new Queue<ShadowItem>();
                var shadowItem = caster.ShadowItems[i];
                for (int j = 0; j < shadowCount; j++)
                {
                    ShadowQueues[i].Enqueue(new ShadowItem(caster, shadowItem));
                }
            }
            
            Ready += () =>
            {
                this.AddProcess(Process, caster.ProcessCallback == ShadowCaster2DProcessCallback.Physics);
            };
            
            TreeExited += () =>
            {
                for (int i = 0; i < ShadowQueues.Length; i++)
                {
                    foreach (var item in ShadowQueues[i]) item.Free();
                    foreach (var item in WorkingQueues[i]) item.Free();
                    ShadowQueues[i].Clear();
                    WorkingQueues[i].Clear();
                }
            };
        }

        public void Emit()
        {
            for (int i = 0; i < ShadowQueues.Length; i++)
            {
                var item = ShadowQueues[i].Dequeue();
                if (item.Init(Counter)) WorkingQueues[i].Enqueue(item);
                else ShadowQueues[i].Enqueue(item);
            }
            
            Counter++;
        }
        
        private int Counter;
        public void ResetIndex() => Counter = 0;

        private void Process(double delta)
        {
            var working = false;
            
            for (int i = 0; i < WorkingQueues.Length; i++)
            {
                var queue = WorkingQueues[i];
                var count = 0;
                foreach (var item in queue)
                {
                    working = true;
                    
                    if (item.Update(delta))
                        count++;
                }

                for (int j = 0; j < count; j++)
                {
                    var item = queue.Dequeue();
                    if (!Cleared) ShadowQueues[i].Enqueue(item);
                    else item.Free();
                }
            }
            
            if (!working && Cleared) QueueFree();
        }
        
        private bool Cleared;
        public void Clear()
        {
            Cleared = true;
            foreach (var queue in ShadowQueues)
            {
                foreach (var item in queue)
                {
                    item.Free();
                }
                queue.Clear();
            }
        }
    }
    
    private static int GetShadowCount(double interval, double shadowTime)
        => (int)Math.Ceiling(shadowTime / interval);
    
    private ShadowNode ShadowProcessor;

    public ShadowCaster2D() : base()
    {
        TreeEntered += () =>
        {
            ShadowProcessor = new(this);
            Root.AddSiblingSafely(ShadowProcessor);
        };
        
        Ready += () =>
        {
            this.ActionRepeat(Interval, () =>
            {
                if (Emitting) Emit();
                else ResetIndex();
            }, ProcessCallback == ShadowCaster2DProcessCallback.Physics);
        };
        
        TreeExited += () =>
        {
            ShadowProcessor.Clear();
        };
    }
    
    public void Emit() => ShadowProcessor.Emit();
    public void ResetIndex() => ShadowProcessor.ResetIndex();
}