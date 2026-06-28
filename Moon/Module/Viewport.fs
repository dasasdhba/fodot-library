module Moon.Module.Viewport

open Fodot
open Fodot.Module
open Godot

/// The viewport is assumed to be a child of either Sprite2D or SubViewportContainer
/// (where the Sprite2D is assumed not using Region, TextureRect is not considered as it's too complicated),
/// otherwise it only returns a scaled transform if it's SubViewport.
/// (Window is not considered as it's too complicated)
let getParentTransform (viewport: Viewport) =
    let parent =
        viewport |> Node.tryGetParent<Node>
    
    let zoom =
        parent
        |> Option.bind tryUnbox<CanvasItem>
        |> Option.bind (CanvasItem.tryGetShaderParamAs<Vector2> "zoom")
        |> Option.defaultValue Vector2.One
    
    let scale =
        match viewport with
        | :? SubViewport as sub when
            sub.Size2DOverride.X > 0 &&
            sub.Size2DOverride.Y > 0 &&
            sub.Size2DOverrideStretch ->
            (Vector2.from sub.Size) / (Vector2.from sub.Size2DOverride)
        | _ -> Vector2.One
    
    let transform =
        Transform2D(0f, zoom * scale, 0f, Vector2.Zero)
    
    let parentGetTransform (size : Vector2) (parent : Node) =
        match parent with
        | :? Sprite2D as spr ->
            let offset =
                if spr.Centered then spr.Offset + size / 2f else spr.Offset
            spr.GlobalTransform.TranslatedLocal offset
            |> Some
        | :? SubViewportContainer as c ->
            c.GetGlobalTransform() |> Some
        | _ -> None
    
    parent
    |> Option.bind (parentGetTransform (viewport.GetVisibleRect().Size))
    |> Option.map (fun pt -> pt * transform)
    |> Option.defaultValue transform