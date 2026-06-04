namespace Fodot.Core

open FSharp.Extend
open Godot

type WeakMeta<'a when 'a : not struct> = WeakMap<GodotObject, 'a>

module WeakMeta =
    
    let remove obj (map: WeakMeta<'a>) =
        map |> WeakMap.remove obj
    
    let update obj value (map: WeakMeta<'a>) =
        map |> WeakMap.update obj value
    
    let addOrUpdate obj value updateFunc (map: WeakMeta<'a>) =
        map |> WeakMap.addOrUpdate obj value updateFunc
    
    let getOrAdd obj (value : Lazy<'a>) (map: WeakMeta<'a>) =
        map |> WeakMap.getOrAdd obj value
            
    let tryGet obj (map: WeakMeta<'a>) =
        map |> WeakMap.tryGetValue obj
        
    let get obj (map: WeakMeta<'a>) =
        map |> WeakMap.getValue obj
        
    let contains obj (map: WeakMeta<'a>) =
        map |> WeakMap.containsKey obj
        
    let tryAdd obj value (map: WeakMeta<'a>) =
        map |> WeakMap.tryAdd obj value