module Moon.Module.CanvasItem

open Fodot
open Fodot.Extend
open Fodot.Module
open Godot

let tryGetTexture (item : CanvasItem) =
    match item with
    | :? Sprite2D as spr -> spr.Texture |> Option.ofObj
    | :? AnimatedSprite2D as anim ->
        anim.SpriteFrames
        |> Option.ofObj
        |> Option.bind(fun f ->
            f.GetFrameTexture(anim.Animation, anim.Frame) |> Option.ofObj
        )
    | _ -> None
    
let fadeIn time (item: CanvasItem) =
    item.Modulate <- item.Modulate |> Color.withA 0f
    let tween = Tween.createPhysicsWith item
    tween
    |> Tween.property item "modulate:a" 1f time
    |> ignore
    
    tween
    
let fadeOut time (item: CanvasItem) =
    let tween = Tween.createPhysicsWith item
    tween
    |> Tween.property item "modulate:a" 0f time
    |> ignore
    
    tween

/// The viewport is assumed to be a child of either Sprite2D or SubViewportContainer
/// (where the Sprite2D is assumed not using Region),
/// otherwise it only returns a transform relative to the viewport origin.
let getGlobalTransformWithViewport (item: CanvasItem) =
    let viewport = item.GetViewport()
    let gt = item.GetGlobalTransformWithCanvas()
    let gt =
        match viewport with
        | :? SubViewport as sub when
            sub.Size2DOverride.X > 0 &&
            sub.Size2DOverride.Y > 0 &&
            sub.Size2DOverrideStretch ->
            let scale = (Vector2.from sub.Size) / (Vector2.from sub.Size2DOverride)
            gt.Scaled scale
        | _ -> gt
    
    let getParentTransform (size : Vector2) (parent : Node) =
        match parent with
        | :? Sprite2D as spr ->
            let offset =
                if spr.Centered then spr.Offset + size / 2f else spr.Offset
            spr.GlobalTransform.TranslatedLocal offset
            |> Some
        | :? SubViewportContainer as c ->
            c.GetGlobalTransform() |> Some
        | _ -> None
    
    viewport
    |> GodotObject.validate
    |> Option.bind Node.tryGetParent<Node>
    |> Option.bind (getParentTransform (viewport.GetVisibleRect().Size))
    |> Option.defaultValue gt