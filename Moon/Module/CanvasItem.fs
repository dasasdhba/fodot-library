module Moon.Module.CanvasItem

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