namespace FSharp.Extend

open System.Runtime.CompilerServices

type WeakMap<'a, 'b when 'a : not struct and 'b : not struct> = ConditionalWeakTable<'a, 'b>

module WeakMap =
    
    let remove key (map: WeakMap<'a, 'b>) =
        map.Remove key
    
    let update key value (map: WeakMap<'a, 'b>) =
        map.AddOrUpdate(key, value)
    
    let addOrUpdate key (value : Lazy<'b>) (updateFunc : 'b -> 'b) (map: WeakMap<'a, 'b>) =
        match map.TryGetValue key with
        | true, v -> map |> update key (updateFunc v)
        | false, _ -> map |> update key value.Value
    
    let getOrAdd key (value : Lazy<'b>) (map: WeakMap<'a, 'b>) =
        match map.TryGetValue key with
        | true, value -> value
        | false, _ ->
            map.Add(key, value.Value)
            value.Value
            
    let tryGetValue key (map: WeakMap<'a, 'b>) =
        match map.TryGetValue key with
        | true, value -> Some value
        | false, _ -> None
        
    let getValue key (map: WeakMap<'a, 'b>) =
        map
        |> tryGetValue key
        |> Option.defaultWith (fun _ -> failwith $"{map}: key {key} not found.")
        
    let containsKey key (map: WeakMap<'a, 'b>) =
        map
        |> tryGetValue key
        |> Option.isSome
        
    let tryAdd key (value : Lazy<'b>) (map: WeakMap<'a, 'b>) =
        if map |> containsKey key then
            false
        else
            map.TryAdd(key, value.Value) 