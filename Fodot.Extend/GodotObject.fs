#nowarn "44"

module Fodot.Extend.GodotObject

open Godot
open Fodot

let validate (obj : 'a) =
    obj
    |> Option.ofObj
    |> Option.filter GodotObject.IsInstanceValid

// script

let tryGetScript<'a> (obj : GodotObject) =
    try
        Some (obj :> obj :?> 'a)
    with
    | _ -> obj |> FScript.tryGet<'a>
        
let getScript<'a> (obj : GodotObject) =
    obj
    |> tryGetScript<'a>
    |> Option.defaultWith (fun () -> failwith $"Object {obj} does not implement {typeof<'a>}")
    
let getAllScripts<'a> (obj : GodotObject) = seq {
    try
        yield obj :> obj :?> 'a
    with
    | _ -> ()

    yield! obj |> FScript.getAll<'a>
}

// data

let setData (name : StringName) (value : 'a) (rid: Rid) (obj : GodotObject) =
    match obj with
    | :? TileMap as tm ->
        let layer = tm.GetLayerForBodyRid rid
        let coord = tm.GetCoordsForBodyRid rid
        let data = tm.GetCellTileData(layer, coord)
        data.SetCustomData(name, value |> Variant.from)
    | :? TileMapLayer as tml ->
        let coord = tml.GetCoordsForBodyRid rid
        let data = tml.GetCellTileData coord
        data.SetCustomData(name, value |> Variant.from)
    | _ ->
        obj |> GodotObject.setMeta name value
    
let hasData (name : StringName) (obj : GodotObject) =
    match obj with
    | :? TileMap as tm when tm.TileSet <> null ->
        tm.TileSet |> TileSet.hasCustomData (name.ToString())
    | :? TileMapLayer as tml when tml.TileSet <> null ->
        tml.TileSet |> TileSet.hasCustomData (name.ToString())
    | _ ->
        obj |> GodotObject.hasMeta name
    
let removeData (name : StringName) (obj : GodotObject) =
    match obj with
    | :? TileMap as tm when tm.TileSet <> null ->
        tm.TileSet |> TileSet.removeCustomData (name.ToString())
    | :? TileMapLayer as tml when tml.TileSet <> null ->
        tml.TileSet |> TileSet.removeCustomData (name.ToString())
    | _ ->
        obj |> GodotObject.removeMeta name
    
let tryGetData (name : StringName) (rid : Rid) (obj : GodotObject) =
    if obj |> hasData name |> not then
        None
    else
        match obj with
        | :? TileMap as tm ->
            let layer = tm.GetLayerForBodyRid rid
            let coord = tm.GetCoordsForBodyRid rid
            let data = tm.GetCellTileData(layer, coord)
            data.GetCustomData(name) |> Some
        | :? TileMapLayer as tml ->
            let coord = tml.GetCoordsForBodyRid rid
            let data = tml.GetCellTileData coord
            data.GetCustomData(name) |> Some
        | _ ->
            obj |> GodotObject.tryGetMeta name
        
let getData (name : StringName) (rid : Rid) (obj : GodotObject) =
    obj
    |> tryGetData name rid
    |> Option.defaultWith (fun () -> failwith $"{obj}: Data {name} not found.")
    
let tryGetDataAs<'a> (name : StringName) (rid : Rid) (obj : GodotObject) =
    obj |> tryGetData name rid |> Option.bind Variant.toSome<'a>
    
let tryGetDataAsArray<'a> (name : StringName) (rid : Rid) (obj : GodotObject) =
    obj |> tryGetData name rid |> Option.bind Variant.toSomeArray<'a>
    
let tryGetDataAsDictionary<'a, 'b> (name : StringName) (rid : Rid) (obj : GodotObject) =
    obj |> tryGetData name rid |> Option.bind Variant.toSomeDictionary<'a, 'b>
    
let getDataAs<'a> (name : StringName) (rid : Rid) (obj : GodotObject) =
    obj |> getData name rid |> Variant.toType<'a>
    
let getDataAsArray<'a> (name : StringName) (rid : Rid) (obj : GodotObject) =
    obj |> getData name rid |> Variant.toArray<'a>
    
let getDataAsDictionary<'a, 'b> (name : StringName) (rid : Rid) (obj : GodotObject) =
    obj |> getData name rid |> Variant.toDictionary<'a, 'b>
   
let private getDefaultDataWith<'a> getter (name : StringName) (defFunc : unit -> 'a) (rid: Rid) (obj : GodotObject) =
    (rid, obj)
    ||> getter name
    |> Option.defaultWith (fun () ->
        let value = defFunc ()
        (rid, obj) ||> setData name value
        value
    )
    
let getDataWithDefaultAs<'a> (name : StringName) (defFunc : unit -> 'a) (rid: Rid) (obj : GodotObject) =
    (rid, obj) ||> getDefaultDataWith tryGetDataAs name defFunc
        
let getDataWithDefaultAsArray<'a> (name : StringName) (defFunc : unit -> Collections.Array<'a>) (rid: Rid) (obj : GodotObject) =
    (rid, obj) ||> getDefaultDataWith tryGetDataAsArray name defFunc
        
let getDataWithDefaultAsDictionary<'a, 'b> (name : StringName) (defFunc : unit -> Collections.Dictionary<'a, 'b>) (rid: Rid) (obj : GodotObject) =
    (rid, obj) ||> getDefaultDataWith tryGetDataAsDictionary name defFunc
