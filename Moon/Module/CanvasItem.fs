module Moon.Module.CanvasItem

open Fodot
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
    
let getStageTransform (item: CanvasItem) =
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

    viewport
    |> Option.ofObj
    |> Option.bind Node.tryGetParent<CanvasItem>
    |> Option.map (fun container ->
        container.GetGlobalTransform() * gt
    )
    |> Option.defaultValue gt