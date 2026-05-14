using System;
using System.Collections.Generic;
using Godot;
using Moon.Utils;

namespace Moon.Class;

/// <summary>
/// Provide a drawing workflow similar to Game Maker.
/// </summary>
public partial class Draw2D : Node2D
{
    /// <summary>
    /// Centered the direct drawing Texture/SpriteFrames.
    /// </summary>
    [ExportCategory("Draw2D")]
    [Export]
    public bool Centered { get; set; } = true;

    /// <summary>
    /// Drawing offset.
    /// </summary>
    [Export]
    public Vector2 Offset { get; set; } = new Vector2(0f, 0f);

    /// <summary>
    /// Flip the drawing result horizontally.
    /// </summary>
    [Export]
    public bool FlipH { get; set; } = false;

    /// <summary>
    /// Flip the drawing result vertically.
    /// </summary>
    [Export]
    public bool FlipV { get; set; } = false;

    /// <summary>
    /// The max count of drawing task.
    /// Changing it in the runtime is invalid.
    /// </summary>
    [Export]
    public int MaxDrawingTask { get; set; } = 32;

    /// <summary>
    /// Process callback mode.
    /// </summary>
    public enum Draw2DProcessCallback
    {
        Idle,
        Physics
    }

    /// <summary>
    /// Draw2D Process callback mode.
    /// </summary>
    [Export]
    public Draw2DProcessCallback ProcessCallback { get; set; } =
        Draw2DProcessCallback.Physics;

    /// <summary>
    /// Run process only if visible.
    /// </summary>
    [Export]
    public bool VisibleOnly { get; set; } = true;

    private List<Action<Rid>> QueuedDrawingTasks = new();
    protected List<Rid> QueuedDrawers { get ;set; } = new();

    public Draw2D() : base()
    {
        TreeEntered += () =>
        {
            var self = GetCanvasItem();
            for (int i = 0; i < MaxDrawingTask; i++)
            {
                var drawer = RenderingServer.CanvasItemCreate();
                RenderingServer.CanvasItemSetParent(drawer, self);
                QueuedDrawers.Add(drawer);
            }
        };
        
        Ready += () =>
        {
            this.AddProcess(ProcessDrawing, ProcessCallback == Draw2DProcessCallback.Physics);
        };
        
        TreeExited += () =>
        {
            foreach (var drawer in QueuedDrawers)
            {
                RenderingServer.FreeRid(drawer);
            }
            
            QueuedDrawers.Clear();
        };

    }

    /// <summary>
    /// Main draw method, called automatically by default process callback mode.
    /// </summary>
    private void Redraw()
    {
        for (int i = 0; i < QueuedDrawers.Count; i++)
        {
            var drawer = QueuedDrawers[i];
            
            if (i >= QueuedDrawingTasks.Count)
            {
                RenderingServer.CanvasItemSetVisible(drawer, false);
                continue;
            }
            
            RenderingServer.CanvasItemClear(drawer);
            RenderingServer.CanvasItemSetVisible(drawer, true);
            QueuedDrawingTasks[i].Invoke(drawer);
        }
        
        QueuedDrawingTasks.Clear();
    }

    protected void ClearDrawSettings()
    {
        ResetBlendMode();
        ResetDrawMaterial();
        ResetDrawModulate();
        ResetDrawTransform();
        ResetDrawZIndex();
    }

    /// <summary>
    /// Create drawing tasks in process.
    /// The base method will clear the tasks,
    /// reset the blend mode, material modulate, transform, etc.
    /// Override to implement.
    /// </summary>
    /// <returns>true to update draw, false to keep last draw.</returns>
    protected virtual bool DrawProcess(double delta)
    {
        ClearDrawSettings();

        return true;
    }

    private void ProcessDrawing(double delta)
    {
        if (VisibleOnly && !IsVisibleInTree()) { return; }

        if (DrawProcess(delta))
            Redraw();
    }
}