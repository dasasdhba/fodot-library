module Moon.Module.CanvasItem

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