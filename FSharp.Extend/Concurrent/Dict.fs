module FSharp.Concurrent.Dict

open System.Collections.Concurrent

let addOrUpdate key value (dict : ConcurrentDictionary<'a, 'b>) =
    dict.AddOrUpdate(key, value, (fun _ _ -> value)) |> ignore

let tryAdd key (value : Lazy<'b>) (dict : ConcurrentDictionary<'a, 'b>) =
    if dict.ContainsKey key then
        false
    else
        dict.TryAdd(key, value.Value)

let tryGetValue key (dict : ConcurrentDictionary<'a, 'b>) =
    match dict.TryGetValue key with
    | true, obj -> Some obj
    | false, _ -> None
    
let tryRemove key (dict : ConcurrentDictionary<'a, 'b>) =
    let mutable v = Unchecked.defaultof<'b>
    match dict.TryRemove(key, &v) with
    | true -> Some v
    | false -> None
    
