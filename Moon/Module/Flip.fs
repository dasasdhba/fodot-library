module Moon.Module.Flip

open Fodot
open Fodot.Extend
open Godot
open Moon.Interface

let initH node =
    let selfInit obj =
        obj
        |> GodotObject.tryGetScript<IFlipHInit>
        |> Option.iter _.InitFlipH()

    node
    |> GodotObject.tryGetScript<IFlipHInit>
    |> Option.iter _.InitFlipH()
    
    node |> Node.getChildrenRecInternal |> Seq.iter selfInit
    
let initV node =
    let selfInit obj =
        obj
        |> GodotObject.tryGetScript<IFlipVInit>
        |> Option.iter _.InitFlipV()
    
    node |> selfInit
    node |> Node.getChildrenRecInternal |> Seq.iter selfInit

let private flipHGd = new StringName "flip_h"
let private flipVGd = new StringName "flip_v"
let private flipHCs = new StringName "FlipH"
let private flipVCs = new StringName "FlipV"

let getH (node : Node) =
    match node with
    | :? Sprite2D as s -> s.FlipH
    | :? AnimatedSprite2D as s -> s.FlipH
    | :? Sprite3D as s -> s.FlipH
    | :? AnimatedSprite3D as s -> s.FlipH
    | _ ->
        node
        |> GodotObject.tryGetAs<bool> flipHGd
        |> Option.orElseWith (fun _ -> node |> GodotObject.tryGetAs<bool> flipHCs)
        |> Option.defaultValue false
    
let getV (node : Node) =
    match node with
    | :? Sprite2D as s -> s.FlipV
    | :? AnimatedSprite2D as s -> s.FlipV
    | :? Sprite3D as s -> s.FlipV
    | :? AnimatedSprite3D as s -> s.FlipV
    | _ ->
        node
        |> GodotObject.tryGetAs<bool> flipVGd
        |> Option.orElseWith (fun _ -> node |> GodotObject.tryGetAs<bool> flipVCs)
        |> Option.defaultValue false
    
let setH (value : bool) (node : Node) =
    match node with
    | :? Sprite2D as s -> s.FlipH <- value
    | :? AnimatedSprite2D as s -> s.FlipH <- value
    | :? Sprite3D as s -> s.FlipH <- value
    | :? AnimatedSprite3D as s -> s.FlipH <- value
    | n when n |> GodotObject.tryGetAs<bool> flipHGd |> Option.isSome ->
        n |> GodotObject.set flipHGd value
    | n when n |> GodotObject.tryGetAs<bool> flipHCs |> Option.isSome ->
        n |> GodotObject.set flipHCs value
    | _ -> ()
    
let setV (value : bool) (node : Node) =
    match node with
    | :? Sprite2D as s -> s.FlipV <- value
    | :? AnimatedSprite2D as s -> s.FlipV <- value
    | :? Sprite3D as s -> s.FlipV <- value
    | :? AnimatedSprite3D as s -> s.FlipV <- value
    | n when n |> GodotObject.tryGetAs<bool> flipVGd |> Option.isSome ->
        n |> GodotObject.set flipVGd value
    | n when n |> GodotObject.tryGetAs<bool> flipVCs |> Option.isSome ->
        n |> GodotObject.set flipVCs value
    | _ -> ()
