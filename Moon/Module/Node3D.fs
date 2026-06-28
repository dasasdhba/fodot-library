module Moon.Module.Node3D

open Fodot.Extend
open Godot

let isBehindCamera (node : Node3D) =
    let viewport = node.GetViewport ()
    let camera = viewport.GetCamera3D ()
    camera
    |> GodotObject.validate
    |> Option.map (fun c -> c.IsPositionBehind node.GlobalPosition)
    |> Option.defaultValue false

/// An active Camera3D is desired, and the viewport is assumed
/// to be a child of either Sprite2D or SubViewportContainer
/// (where the Sprite2D is assumed not using Region),
/// otherwise it only returns a Vector.ZERO
let getGlobalPositionWithViewport (node : Node3D) =
    let viewport = node.GetViewport ()
    let camera = viewport.GetCamera3D ()
    let gt =
        camera
        |> GodotObject.validate
        |> Option.map (fun c -> c.UnprojectPosition node.GlobalPosition)
        |> Option.defaultValue Vector2.Zero
    
    viewport
    |> GodotObject.validate
    |> Option.map Viewport.getParentTransform
    |> Option.map (fun pt -> pt * gt)
    |> Option.defaultValue gt