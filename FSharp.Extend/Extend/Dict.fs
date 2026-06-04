module FSharp.Extend.Dict

open System
open System.Collections.Generic

let remove (key: 'a) (dict : IDictionary<'a, 'b>) =
    dict.Remove key

let update key (value : 'b) (dict: IDictionary<'a, 'b>) =
    dict[key] <- value

let addOrUpdate key (value : Lazy<'b>) (updateFunc: 'b -> 'b) (dict: IDictionary<'a, 'b>) =
    match dict.TryGetValue key with
    | true, v -> dict |> update key (updateFunc v)
    | _ -> dict |> update key value.Value

let getOrAdd key (value : Lazy<'b>) (dict: IDictionary<'a, 'b>) =
    match dict.TryGetValue key with
    | true, v -> v
    | _ ->
        dict.Add(key, value.Value)
        value.Value
        
let tryGetValue key (dict: IDictionary<'a, 'b>) =
    match dict.TryGetValue key with
    | true, v -> Some v
    | _ -> None

let getValue key (dict: IDictionary<'a, 'b>) =
    dict[key]
    
let containsKey key (dict: IDictionary<'a, 'b>) =
    dict.ContainsKey key
    
let tryAdd key (value: Lazy<'b>) (dict: IDictionary<'a, 'b>) =
    if dict.ContainsKey key then
        false
    else
        dict.TryAdd(key, value.Value)