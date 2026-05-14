using System;
using System.Collections.Generic;
using Godot;

namespace Moon.Class;

public partial class Draw2D : Node2D
{
    // drawing API used in DrawProcess call.

    protected enum Draw2DBlendMode
    {
        Mix,
        Add,
        Sub,
        Mul,
        PremultAlpha
    }

    private Draw2DBlendMode BlendMode = Draw2DBlendMode.Mix;

    /// <summary>
    /// Blend mode will be overridden by custom material.
    /// </summary>
    protected void SetBlendMode(Draw2DBlendMode blendMode) => BlendMode = blendMode;
    protected void ResetBlendMode() => BlendMode = Draw2DBlendMode.Mix;

    private Dictionary<Draw2DBlendMode, Material> BlendMaterialMap = new()
    {
        { Draw2DBlendMode.Mix, new CanvasItemMaterial() {
            BlendMode = CanvasItemMaterial.BlendModeEnum.Mix }
        },
        { Draw2DBlendMode.Add, new CanvasItemMaterial() { 
            BlendMode = CanvasItemMaterial.BlendModeEnum.Add } 
        },
        { Draw2DBlendMode.Sub, new CanvasItemMaterial() {
            BlendMode = CanvasItemMaterial.BlendModeEnum.Sub } 
        },
        { Draw2DBlendMode.Mul, new CanvasItemMaterial() {
            BlendMode = CanvasItemMaterial.BlendModeEnum.Mul } 
        },
        { Draw2DBlendMode.PremultAlpha, new CanvasItemMaterial() {
            BlendMode = CanvasItemMaterial.BlendModeEnum.PremultAlpha } 
        }
    };

    private Material DrawMaterial = null;

    /// <summary>
    /// Non-null material will override blend mode setting.
    /// </summary>
    protected void SetDrawMaterial(Material material) => DrawMaterial = material;
    protected void ResetDrawMaterial() => DrawMaterial = null;

    private Color DrawModulate = new(1f, 1f, 1f);

    protected void SetDrawModulate(Color modulate) => DrawModulate = modulate;
    protected void SetDrawModulateRGB(Color modulate)
    {
        DrawModulate.R = modulate.R;
        DrawModulate.G = modulate.G;
        DrawModulate.B = modulate.B;
    }
    protected void SetDrawModulateAlpha(float alpha) => DrawModulate.A = alpha;
    protected void ResetDrawModulate() => DrawModulate = new(1f, 1f, 1f);

    private Transform2D DrawTransform = new(0f, new Vector2(0f, 0f));
    private bool DrawGlobal = false;
    
    protected void SetDrawGlobal(bool global) => DrawGlobal = global;
    protected void SetDrawTransform(Transform2D transform) => DrawTransform = transform;
    protected void ResetDrawTransform()
    {
        DrawTransform = new(0f, new Vector2(0f, 0f));
        DrawGlobal = false;
    }
    protected void SetDrawPosition(Vector2 pos) => DrawTransform.Origin = pos;
    protected void SetDrawRotation(float rotation) => DrawTransform = new(rotation, 
        DrawTransform.Scale, DrawTransform.Skew, DrawTransform.Origin);
    protected void SetDrawScale(Vector2 scale) => DrawTransform = new(DrawTransform.Rotation,
        scale, DrawTransform.Skew, DrawTransform.Origin);
    protected void SetDrawSkew(float skew) => DrawTransform = new(DrawTransform.Rotation,
        DrawTransform.Scale, skew, DrawTransform.Origin);

    private int DrawZIndex = 0;
    protected void SetDrawZIndex(int zIndex) => DrawZIndex = zIndex;
    protected void ResetDrawZIndex() => DrawZIndex = 0;

    protected void AddDrawingTask(Action<Rid> task)
    {
        var queuedMaterial = (DrawMaterial ?? BlendMaterialMap[BlendMode]).GetRid();
        var queuedModulate = DrawModulate;
        int queuedZIndex = DrawZIndex;
        
        var queuedTransform = DrawTransform;
        var scale = queuedTransform.Scale;
        var position = queuedTransform.Origin;
        
        if (FlipH)
        {
            scale.X *= -1f;
            position.X *= -1f;
        }

        if (FlipV)
        {
            scale.Y *= -1f;
            position.Y *= -1f;
        }
        
        queuedTransform = new(queuedTransform.Rotation, scale,
            queuedTransform.Skew, position + Offset);

        if (DrawGlobal)
        {
            queuedTransform = GlobalTransform.AffineInverse() * queuedTransform;
        }

        QueuedDrawingTasks.Add((drawer) =>
        {
            RenderingServer.CanvasItemSetMaterial(drawer, queuedMaterial);
            RenderingServer.CanvasItemSetModulate(drawer, queuedModulate);
            RenderingServer.CanvasItemSetZIndex(drawer, queuedZIndex);
            RenderingServer.CanvasItemSetTransform(drawer, queuedTransform);
            
            task.Invoke(drawer);
        });
    }

    // line

    /// <summary>
    /// <inheritdoc cref="Godot.CanvasItem.DrawLine(Vector2, Vector2, Color, float, bool)"/>
    /// </summary>
    protected void QueuedDrawLine(Vector2 from, Vector2 to, Color color, 
        float width = -1f, bool antialiased = false)
    {
        AddDrawingTask((drawer) =>
        {
            RenderingServer.CanvasItemAddLine(drawer, from, to, color, width, antialiased);
        });
    }
    
    /// <summary>
    /// <inheritdoc cref="Godot.CanvasItem.DrawMultiline(Vector2[], Color, float)"/>
    /// </summary>
    protected void QueuedDrawMultiline(Vector2[] points, Color color, float width = -1f, bool antialiased = false)
    {
        var colors = new Color[points.Length - 1];
        Array.Fill(colors, color);
        
        AddDrawingTask((drawer) =>
        {
            RenderingServer.CanvasItemAddMultiline(drawer, points, colors, width, antialiased);
        });
    }
    
    /// <summary>
    /// <inheritdoc cref="Godot.CanvasItem.DrawMultilineColors(Vector2[], Color[], float)"/>
    /// </summary>
    protected void QueuedDrawMultilineColors(Vector2[] points, Color[] colors, float width = -1f, bool antialiased = false)
    {
        AddDrawingTask((drawer) =>
        {
            RenderingServer.CanvasItemAddMultiline(drawer, points, colors, width, antialiased);
        });
    }
    
    /// <summary>
    /// <inheritdoc cref="Godot.CanvasItem.DrawPolyline(Vector2[], Color, float, bool)"/>
    /// </summary>
    protected void QueuedDrawPolyline(Vector2[] points, Color color,
        float width = -1f, bool antialiased = false)
    {
        var colors = new Color[points.Length];
        Array.Fill(colors, color);
    
        AddDrawingTask((drawer) =>
        {
            RenderingServer.CanvasItemAddPolyline(drawer, points, colors, width, antialiased);
        });
    }
    
    /// <summary>
    /// <inheritdoc cref="Godot.CanvasItem.DrawPolylineColors(Vector2[], Color[], float, bool)"/>
    /// </summary>
    protected void QueuedDrawPolylineColors(Vector2[] points, Color[] colors,
        float width = -1f, bool antialiased = false)
    {
        AddDrawingTask((drawer) =>
        {
            RenderingServer.CanvasItemAddPolyline(drawer, points, colors, width, antialiased);
        });
    }

    // shape
    
    /// <summary>
    /// <inheritdoc cref="Godot.CanvasItem.DrawArc(Vector2, float, float, float, int, Color, float, bool)"/>
    /// </summary>
    protected void QueuedDrawArc(Vector2 center, float radius, float startAngle, float endAngle,
        Color color, int pointCount = 128, float width = -1f, bool antialiased = false)
    {
        var points = new Vector2[pointCount];
        
        var deltaAngle = Math.Clamp(endAngle - startAngle, -Mathf.Pi, Mathf.Pi);
        for (int i = 0; i < pointCount; i++)
        {
            var theta = (i / (pointCount - 1f)) * deltaAngle + startAngle;
            points[i] = center + new Vector2((float)Math.Cos(theta), (float)Math.Sin(theta)) * radius;
        }
    
        QueuedDrawPolyline(points, color, width, antialiased);
    }
    
    /// <summary>
    /// <inheritdoc cref="Godot.CanvasItem.DrawCircle(Vector2, float, Color)"/>
    /// </summary>
    protected void QueuedDrawCircle(Vector2 center, float radius, Color color, bool anitiliased = false)
    {
        AddDrawingTask((drawer) =>
        {
            RenderingServer.CanvasItemAddCircle(drawer, center, radius, color, anitiliased);
        });
    }
    
    /// <summary>
    /// <inheritdoc cref="Godot.CanvasItem.DrawRect(Rect2, Color, bool, float)"/>
    /// </summary>
    protected void QueuedDrawRect(Rect2 rect, Color color, bool antialiased = false)
    {
        AddDrawingTask((drawer) =>
        {
            RenderingServer.CanvasItemAddRect(drawer, rect, color, antialiased);
        });
    }
    
    /// <summary>
    /// <inheritdoc cref="Godot.CanvasItem.DrawPolygon(Vector2[], Color[], Vector2[], Texture2D)"/>
    /// </summary>
    protected void QueuedDrawPolygon(Vector2[] points, Color[] colors,
        Vector2[] uvs = null, Texture2D texture = null)
    {
        var texId = texture?.GetRid() ?? default;
        
        AddDrawingTask((drawer) =>
        {
            RenderingServer.CanvasItemAddPolygon(drawer, points, colors, uvs, texId);
        });
    }
    
    /// <summary>
    /// <inheritdoc cref="Godot.CanvasItem.DrawColoredPolygon(Vector2[], Color, Vector2[], Texture2D)"/>
    /// </summary>
    protected void QueuedDrawColoredPolygon(Vector2[] points, Color color,
        Vector2[] uvs = null, Texture2D texture = null)
    {
        var colors = new Color[points.Length];
        Array.Fill(colors, color);
        
        QueuedDrawPolygon(points, colors, uvs, texture);
    }
    
    /// <summary>
    /// <inheritdoc cref="Godot.CanvasItem.DrawPrimitive(Vector2[], Color[], Vector2[], Texture2D)"/>
    /// </summary>
    protected void QueuedDrawPrimitive(Vector2[] points, Color[] colors,
        Vector2[] uvs = null, Texture2D texture = null)
    {
        var texId = texture?.GetRid() ?? default;
    
        AddDrawingTask((drawer) =>
        {
            RenderingServer.CanvasItemAddPrimitive(drawer, points, colors, uvs, texId);
        });
    }

    // texture
    
    /// <summary>
    /// <inheritdoc cref="Godot.CanvasItem.DrawTexture(Texture2D, Vector2, Color?)"/>
    /// </summary>
    protected void QueuedDrawTexture(Texture2D texture, Vector2 pos, Color? modulate = null)
    {
        if (texture == null) return;
        
        if (Centered)
        {
            var texSize = new Vector2(texture.GetWidth(), texture.GetHeight());
            pos -= texSize / 2f;
        }
        
        AddDrawingTask((drawer) =>
        {
            texture.Draw(drawer, pos, modulate, false);
        });
    }
    
    /// <summary>
    /// <inheritdoc cref="Godot.CanvasItem.DrawTextureRect(Texture2D, Rect2, bool, Color?, bool)"/>
    /// </summary>
    protected void QueuedDrawTextureRect(Texture2D texture, Rect2 rect, bool tile,
        Color? modulate = null, bool transpose = false)
    {
        if (texture == null) return;
        
        AddDrawingTask((drawer) =>
        {
            texture.DrawRect(drawer, rect, tile, modulate, transpose);
        });
    }
    
    /// <summary>
    /// <inheritdoc cref="Godot.CanvasItem.DrawTextureRectRegion(Texture2D, Rect2, Rect2, Color?, bool, bool)"/>
    /// </summary>
    protected void QueuedDrawTextureRectRegion(Texture2D texture, Rect2 rect, Rect2 srcRect,
        Color? modulate = null, bool transpose = false, bool clipUV = true)
    {
        if (texture == null) return;

        AddDrawingTask((drawer) =>
        {
            texture.DrawRectRegion(drawer, rect, srcRect, modulate, transpose, clipUV);
        });
    }

    // sprite frames
    
    /// <summary>
    /// <inheritdoc cref="QueuedDrawTexture"/>
    /// </summary>
    protected void QueuedDrawSpriteFrames(SpriteFrames spr, string animation, int frame, 
        Vector2 pos, Color? modulate = null)
    {
        var texture = spr.GetFrameTexture(animation, frame);
        QueuedDrawTexture(texture, pos, modulate);
    }
    
    /// <summary>
    /// <inheritdoc cref="QueuedDrawTextureRect"/>
    /// </summary>
    protected void QueuedDrawSpriteFramesRect(SpriteFrames spr, string animation, int frame,
        Rect2 rect, bool tile, Color? modulate = null, bool transpose = false)
    {
        var texture = spr.GetFrameTexture(animation, frame);
        QueuedDrawTextureRect(texture, rect, tile, modulate, transpose);
    }
    
    /// <summary>
    /// <inheritdoc cref="QueuedDrawTextureRectRegion"/>
    /// </summary>
    protected void QueuedDrawSpriteFramesRectRegion(SpriteFrames spr, string animation, int frame,
        Rect2 rect, Rect2 srcRect, Color? modulate = null, 
        bool transpose = false, bool clipUV = true)
    {
        var texture = spr.GetFrameTexture(animation, frame);
        QueuedDrawTextureRectRegion(texture, rect, srcRect, modulate, transpose, clipUV);
    }

    // sprite2d
    
    /// <summary>
    /// <inheritdoc cref="QueuedDrawTexture"/>
    /// </summary>
    protected void QueuedDrawSprite(Sprite2D spr, Vector2 pos, Color? modulate = null)
    {
        pos += spr.Offset;
        QueuedDrawTexture(spr.Texture, pos, modulate);
    }
    
    /// <summary>
    /// <inheritdoc cref="QueuedDrawTextureRect"/>
    /// </summary>
    protected void QueuedDrawSpriteRect(Sprite2D spr, Rect2 rect, bool tile,
        Color? modulate = null, bool transpose = false)
    {
        QueuedDrawTextureRect(spr.Texture, rect, tile, modulate);
    }
    
    /// <summary>
    /// <inheritdoc cref="QueuedDrawTextureRectRegion"/>
    /// </summary>
    protected void QueuedDrawSpriteRectRegion(Sprite2D spr, Rect2 rect, Rect2 srcRect,
        Color? modulate = null, bool transpose = false, bool clipUV = true)
    {
        QueuedDrawTextureRectRegion(spr.Texture, rect, srcRect, modulate, transpose, clipUV);
    }

    // animated sprite
    
    /// <summary>
    /// <inheritdoc cref="QueuedDrawSpriteFrames"/>
    /// </summary>
    protected void QueuedDrawAnimatedSprite(AnimatedSprite2D spr, Vector2 pos, Color? modulate = null)
    {
        pos += spr.Offset;
        QueuedDrawSpriteFrames(spr.SpriteFrames, spr.Animation, spr.Frame, pos, modulate);
    }
    
    /// <summary>
    /// <inheritdoc cref="QueuedDrawSpriteFramesRect"/>
    /// </summary>
    protected void QueuedDrawAnimatedSpriteRect(AnimatedSprite2D spr, Rect2 rect, bool tile,
        Color? modulate = null, bool transpose = false)
    { 
        QueuedDrawSpriteFramesRect(spr.SpriteFrames, spr.Animation, spr.Frame, 
            rect, tile, modulate, transpose);
    }

    /// <summary>
    /// <inheritdoc cref="QueuedDrawSpriteFramesRectRegion"/>
    /// </summary>
    protected void QueuedDrawAnimatedSpriteRectRegion(AnimatedSprite2D spr, Rect2 rect, Rect2 srcRect,
        Color? modulate = null, bool transpose = false, bool clipUV = true)
    {
        QueuedDrawSpriteFramesRectRegion(spr.SpriteFrames, spr.Animation, spr.Frame,
            rect, srcRect, modulate, transpose, clipUV);
    }
}