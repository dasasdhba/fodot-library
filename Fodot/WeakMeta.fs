namespace Fodot

open FSharp.Extend
open Godot

type WeakMeta<'a when 'a : not struct> = WeakMap<GodotObject, 'a>

type WeakBox<'a>(value : 'a) =
    member val Value = value
    
type WeakMetaValue<'a> = WeakMeta<WeakBox<'a>>

module WeakMeta =
    
    let remove obj (map: WeakMeta<'a>) =
        map |> WeakMap.remove obj
    
    let update obj value (map: WeakMeta<'a>) =
        map |> WeakMap.update obj value
    
    let updateValue obj (value: 'a) (map: WeakMetaValue<'a>) =
        map |> WeakMap.update obj (WeakBox<'a> value)
    
    let addOrUpdate obj valueFunc updateFunc (map: WeakMeta<'a>) =
        map |> WeakMap.addOrUpdate obj valueFunc updateFunc
    
    let addOrUpdateValue obj (valueFunc: unit -> 'a) updateFunc (map: WeakMetaValue<'a>) =
        map |> WeakMap.addOrUpdate obj (fun () -> WeakBox<'a> (valueFunc ())) (fun v -> WeakBox<'a> (updateFunc v.Value))
    
    let getOrAdd obj (valueFunc : unit -> 'a) (map: WeakMeta<'a>) =
        map |> WeakMap.getOrAdd obj valueFunc

    let getOrAddValue obj (valueFunc : unit -> 'a) (map: WeakMetaValue<'a>) =
        map |> WeakMap.getOrAdd obj (fun () -> WeakBox<'a> (valueFunc ()))

    let tryGet obj (map: WeakMeta<'a>) =
        map |> WeakMap.tryGetValue obj

    let tryGetValue obj (map: WeakMetaValue<'a>) =
        map
        |> WeakMap.tryGetValue obj
        |> Option.map _.Value

    let get obj (map: WeakMeta<'a>) =
        map |> WeakMap.getValue obj
     
    let getValue obj (map: WeakMetaValue<'a>) =
        map |> WeakMap.getValue obj |> _.Value
        
    let contains obj (map: WeakMeta<'a>) =
        map |> WeakMap.containsKey obj
        
    let tryAdd obj valueFunc (map: WeakMeta<'a>) =
        map |> WeakMap.tryAdd obj valueFunc
        
    let tryAddValue obj (valueFunc : unit -> 'a) (map : WeakMetaValue<'a>) =
        map |> WeakMap.tryAdd obj (fun () -> WeakBox<'a> (valueFunc ()))
