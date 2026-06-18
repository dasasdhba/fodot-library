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
    
    let addOrUpdate obj value updateFunc (map: WeakMeta<'a>) =
        map |> WeakMap.addOrUpdate obj value updateFunc
    
    let addOrUpdateValue obj (value: Lazy<'a>) updateFunc (map: WeakMetaValue<'a>) =
        map |> WeakMap.addOrUpdate obj (lazy (WeakBox<'a> value.Value)) (fun v -> WeakBox<'a> (updateFunc v.Value))
    
    let getOrAdd obj (value : Lazy<'a>) (map: WeakMeta<'a>) =
        map |> WeakMap.getOrAdd obj value

    let getOrAddValue obj (value : Lazy<'a>) (map: WeakMetaValue<'a>) =
        map |> WeakMap.getOrAdd obj (lazy (WeakBox<'a> value.Value))

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
        
    let tryAdd obj value (map: WeakMeta<'a>) =
        map |> WeakMap.tryAdd obj value
        
    let tryAddValue obj (value : Lazy<'a>) (map : WeakMetaValue<'a>) =
        map |> WeakMap.tryAdd obj (lazy (WeakBox<'a> value.Value))
