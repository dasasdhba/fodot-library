namespace FSharp.Extend

open System.Runtime.CompilerServices

type WeakMap<'a, 'b when 'a : not struct and 'b : not struct> = ConditionalWeakTable<'a, 'b>

module WeakMap =
    
    let remove key (map: WeakMap<'a, 'b>) =
        map.Remove key
    
    let update key value (map: WeakMap<'a, 'b>) =
        map.AddOrUpdate(key, value)
    
    let addOrUpdate key (valueFunc: unit -> 'b) (updateFunc : 'b -> 'b) (map: WeakMap<'a, 'b>) =
        match map.TryGetValue key with
        | true, v -> map |> update key (updateFunc v)
        | _ -> map |> update key (valueFunc ())
    
    let getOrAdd key (valueFunc: unit -> 'b) (map: WeakMap<'a, 'b>) =
        match map.TryGetValue key with
        | true, value -> value
        | _ ->
            let value = valueFunc ()
            map.Add(key, value)
            value
            
    let tryGetValue key (map: WeakMap<'a, 'b>) =
        match map.TryGetValue key with
        | true, value -> Some value
        | _ -> None
        
    let getValue key (map: WeakMap<'a, 'b>) =
        map
        |> tryGetValue key
        |> Option.defaultWith (fun _ -> failwith $"{map}: Key {key} not found.")
        
    let containsKey key (map: WeakMap<'a, 'b>) =
        map
        |> tryGetValue key
        |> Option.isSome
        
    let tryAdd key (valueFunc : unit -> 'b) (map: WeakMap<'a, 'b>) =
        if map |> containsKey key then
            false
        else
            map.TryAdd(key, valueFunc ())
