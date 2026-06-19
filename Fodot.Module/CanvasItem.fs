module Fodot.Module.CanvasItem

open Fodot
open Godot

// shader

let hasShaderParam (name : string) (item : CanvasItem) =
    match item.Material with
    | :? ShaderMaterial as s ->
        (s.GetShaderParameter name).VariantType <> Variant.Type.Nil
    | _ -> false

let setShaderParam (name : string) (value : 'a) (item : CanvasItem) =
    if item |> hasShaderParam name |> not then
        failwith $"{item} does not contains a valid ShaderMaterial"
    (item.Material :?> ShaderMaterial).SetShaderParameter(name, Variant.from value)

let getShaderParam (name : string) (item : CanvasItem) =
    if item |> hasShaderParam name |> not then
        failwith $"{item} does not contains a valid ShaderMaterial"
    (item.Material :?> ShaderMaterial).GetShaderParameter name

let getShaderParamAs<'a> (name : string) (item : CanvasItem) =
    item |> getShaderParam name |> Variant.toType<'a>

let getShaderParamAsArray<'a> (name : string) (item : CanvasItem) =
    item |> getShaderParam name |> Variant.toArray<'a>

let getShaderParamAsDictionary<'a, 'b> (name : string) (item : CanvasItem) =
    item |> getShaderParam name |> Variant.toDictionary<'a, 'b>

let tryGetShaderParam (name : string) (item : CanvasItem) =
    if item |> hasShaderParam name then
        item |> getShaderParam name |> Some
    else
        None

let tryGetShaderParamAs<'a> (name : string) (item : CanvasItem) =
    item |> tryGetShaderParam name |> Option.bind (fun r -> r |> Variant.toSome<'a>)

let tryGetShaderParamAsArray<'a> (name : string) (item : CanvasItem) =
    item |> tryGetShaderParam name |> Option.bind (fun r -> r |> Variant.toSomeArray<'a>)

let tryGetShaderParamAsDictionary<'a, 'b> (name : string) (item : CanvasItem) =
    item |> tryGetShaderParam name |> Option.bind (fun r -> r |> Variant.toSomeDictionary<'a, 'b>)

// transform

let getTransform (item : CanvasItem) =
    item.GetTransform()

let setTransform (v : Transform2D)  (item : CanvasItem)=
    match item with
    | :? Node2D as n2d ->
        n2d.Transform <- v
    | _ ->
        item |> GodotObject.set Node2D.PropertyName.Transform v

let getPosition (item : CanvasItem) =
    match item with
    | :? Node2D as n2d ->
        n2d.Position
    | :? Control as ctrl ->
        ctrl.Position
    | _ ->
        item |> getTransform |> _.Origin

let setPosition pos (item : CanvasItem) =
    match item with
    | :? Node2D as n2d ->
        n2d.Position <- pos
    | :? Control as ctrl ->
        ctrl.Position <- pos
    | _ ->
        item |> setTransform (item |> getTransform |> Transform2D.withOrigin pos)

let getRotation (item : CanvasItem) =
    match item with
    | :? Node2D as n2d ->
        n2d.Rotation
    | :? Control as ctrl ->
        ctrl.Rotation
    | _ ->
        item |> getTransform |> _.Rotation

let setRotation rot (item: CanvasItem) =
    match item with
    | :? Node2D as n2d ->
        n2d.Rotation <- rot
    | :? Control as ctrl ->
        ctrl.Rotation <- rot
    | _ ->
        item |> setTransform (item |> getTransform |> Transform2D.withRotation rot)

let getScale (item : CanvasItem) =
    match item with
    | :? Node2D as n2d ->
        n2d.Scale
    | :? Control as ctrl ->
        ctrl.Scale
    | _ ->
        item |> getTransform |> _.Scale
    
let setScale scale (item: CanvasItem) =
    match item with
    | :? Node2D as n2d ->
        n2d.Scale <- scale
    | :? Control as ctrl ->
        ctrl.Scale <- scale
    | _ ->
        item |> setTransform (item |> getTransform |> Transform2D.withScale scale)
    
let getSkew (item: CanvasItem) =
    match item with
    | :? Node2D as n2d ->
        n2d.Skew
    | _ ->
        item |> getTransform |> _.Skew

let setSkew skew (item: CanvasItem) =
    match item with
    | :? Node2D as n2d ->
        n2d.Skew <- skew
    | _ ->
        item |> setTransform (item |> getTransform |> Transform2D.withSkew skew)

let getGlobalTransform (item : CanvasItem) =
    item.GetGlobalTransform()
    
let setGlobalTransform (v : Transform2D)  (item : CanvasItem)=
    match item with
    | :? Node2D as n2d ->
        n2d.GlobalTransform <- v
    | _ ->
        item |> GodotObject.set Node2D.PropertyName.GlobalTransform v

let getGlobalPosition (item : CanvasItem) =
    match item with
    | :? Node2D as n2d ->
        n2d.GlobalPosition
    | :? Control as ctrl ->
        ctrl.GlobalPosition
    | _ ->
        item |> getGlobalTransform |> _.Origin

let setGlobalPosition pos (item: CanvasItem) =
    match item with
    | :? Node2D as n2d ->
        n2d.GlobalPosition <- pos
    | :? Control as ctrl ->
        ctrl.GlobalPosition <- pos
    | _ ->
        item |> setGlobalTransform (item |> getGlobalTransform |> Transform2D.withOrigin pos)

let getGlobalRotation (item : CanvasItem) =
    match item with
    | :? Node2D as n2d ->
        n2d.GlobalRotation
    | _ ->
        item |> getGlobalTransform |> _.Rotation

let setGlobalRotation rot (item : CanvasItem) =
    match item with
    | :? Node2D as n2d ->
        n2d.GlobalRotation <- rot
    | _ ->
        item |> setGlobalTransform (item |> getGlobalTransform |> Transform2D.withRotation rot)

let getGlobalScale (item : CanvasItem) =
    match item with
    | :? Node2D as n2d ->
        n2d.GlobalScale
    | _ ->
        item |> getGlobalTransform |> _.Scale
    
let setGlobalScale scale (item: CanvasItem) =
    match item with
    | :? Node2D as n2d ->
        n2d.GlobalScale <- scale
    | _ ->
        item |> setGlobalTransform (item |> getGlobalTransform |> Transform2D.withScale scale)
    
let getGlobalSkew (item: CanvasItem) =
    match item with
    | :? Node2D as n2d ->
        n2d.GlobalSkew
    | _ ->
        item |> getGlobalTransform |> _.Skew

let setGlobalSkew skew (item: CanvasItem) =
    match item with
    | :? Node2D as n2d ->
        n2d.GlobalSkew <- skew
    | _ ->
        item |> setGlobalTransform (item |> getGlobalTransform |> Transform2D.withSkew skew)
