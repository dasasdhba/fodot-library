using System;
using System.Collections.Immutable;
using System.Text;
using Godot;
using Godot.Collections;
using Moon.Utils;

namespace Moon.Class;

// cannot draw atlas texture tiled
// we have to draw separately

[GlobalClass, Tool]
#if TOOLS
public partial class FramesFill : NodeSize2D, ISerializationListener
#else
public partial class FramesFill : NodeSize2D
#endif
{
    [ExportCategory("FramesFill")]
    [Export]
    public SpriteFrames Frames
    {
        get => _frames;
        set
        {
            _frames = value;
        #if TOOLS
            NotifyPropertyListChanged();
        #endif
            Animation = _frames.GetAnimationNames()[0];
            Reset();
            QueueRedraw();
        }
    }
    private SpriteFrames _frames;

    [Export]
    public StringName Animation
    {
        get => _animation;
        set
        {
            _animation = value;
            Reset();
            QueueRedraw();
        }
    }
    private StringName _animation = "Default";
    
    [Export]
    public float SpeedScale { get ;set; } = 1f;
    
    [Export]
    public bool Paused { get ;set; }

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

    public FramesFill() : base()
    {
        TreeEntered += QueueRedraw;
        SignalSizeChanged += QueueRedraw;
        
    #if TOOLS
        if (Engine.IsEditorHint()) return;
    #endif    
        
        Ready += () => this.AddPhysicsProcess(Animate);
    }
    
    private int Frame;
    private float Progress;

    private void Reset()
    {
        Frame = 0;
        Progress = 0f;
    }

    private void Animate(double delta)
    {
        if (Paused) return;
        if (Frames == null) return;
        if (!Frames.HasAnimation(Animation)) return;
        
        var fc = Frames.GetFrameCount(Animation);
        var lastFrame = fc - 1;
        var animSpeed = Frames.GetAnimationSpeed(Animation) * SpeedScale;
        var frameSpeed = 1f / Frames.GetFrameDuration(Animation, Frame);
        var i = 0;
        var last = Frame;
        while (delta > 0d)
        {
            var speed = animSpeed * frameSpeed;
            var absSpeed = Math.Abs(speed);

            if (speed == 0d)
            {
                return; // Do nothing.
            }
            
            if (speed > 0f)
            {
                // Forwards.
                if (Progress >= 1f) 
                {
                    if (Frame >= lastFrame) 
                    {
                        Frame = 0;
                    } 
                    else 
                    {
                        Frame++;
                    }
                    frameSpeed = 1f / Frames.GetFrameDuration(Animation, Frame);
                    Progress = 0f;
                }
                var toProcess = Math.Min((1f - Progress) / absSpeed, delta);
                Progress += (float)(toProcess * absSpeed);
                delta -= toProcess;
            } 
            else 
            {
                // Backwards.
                if (Progress <= 0f) 
                {
                    if (Frame <= 0) 
                    {
                        Frame = lastFrame;
                    } 
                    else 
                    {
                        Frame--;
                    }
                    frameSpeed = 1f / Frames.GetFrameDuration(Animation, Frame);
                    Progress = 1f;
                }
                var toProcess = Math.Min(Progress / absSpeed, delta);
                Progress -= (float)(toProcess * absSpeed);
                delta -= toProcess;
            }

            i++;
            if (i > fc) 
            {
                break; // Prevents freezing if to_process is each time much less than remaining.
            }
        }
        
        if (Frame != last) QueueRedraw();
    }

    public override void _Draw()
    {
        if (Frames == null) return;
        if (!Frames.HasAnimation(Animation)) return;
        
        var texture = Frames.GetFrameTexture(Animation, Frame);
        if (texture == null ) return;
        
        var size = Size;
        if (FlipH) size.X *= -1f;
        if (FlipV) size.Y *= -1f;
        this.DrawTextureRectTiled(texture, new(Vector2.Zero, size));
    }

#if TOOLS
    public override void _PhysicsProcess(double delta)
    {
        if (!Engine.IsEditorHint()) return;
        Animate(delta);
    }

    public override void _ValidateProperty(Dictionary property)
    {
        base._ValidateProperty(property);
        if ((string)property["name"] is "Animation")
        {
            property["hint"] = (uint)PropertyHint.Enum;
            if (Frames == null)
            {
                property["hint_string"] = "Default";
            }
            else
            {
                var names = Frames.GetAnimationNames()
                    .ToImmutableSortedSet();
                var sb = new StringBuilder();
                for (int i = 0; i < names.Count; i++)
                {
                    sb.Append(names[i]);
                    if (i < names.Count - 1)
                    {
                        sb.Append(',');
                    }
                }
                property["hint_string"] = sb.ToString();
            }
        }
    }
    
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