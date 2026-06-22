module FSharp.Concurrent.Dict

open System.Collections.Concurrent

let update key value (dict : ConcurrentDictionary<'a, 'b>) =
    dict[key] <- value

let containsKey key (dict : ConcurrentDictionary<'a, 'b>) =
    dict.ContainsKey key

let tryAdd key (valueFunc : unit -> 'b) (dict : ConcurrentDictionary<'a, 'b>) =
    if dict.ContainsKey key then
        false
    else
        dict.TryAdd(key, valueFunc ())

let tryGetValue key (dict : ConcurrentDictionary<'a, 'b>) =
    match dict.TryGetValue key with
    | true, obj -> Some obj
    | false, _ -> None
    
let tryRemove key (dict : ConcurrentDictionary<'a, 'b>) =
    let mutable v = Unchecked.defaultof<'b>
    match dict.TryRemove(key, &v) with
    | true -> Some v
    | false -> None
    
let remove key (dict : ConcurrentDictionary<'a, 'b>) =
    dict |> tryRemove key |> Option.isSome
    
