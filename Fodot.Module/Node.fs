module Fodot.Module.Node

open Godot
open Fodot.Core

let rec findParentWith<'a when 'a : null and 'a :> Node> filter (node : Node) =
    match node |> Node.tryGetParent<'a> with
    | None -> None
    | Some p when filter p -> Some p
    | Some p -> p |> findParentWith<'a> filter
    
let findParent<'a when 'a : null and 'a :> Node> (node : Node) =
    node |> findParentWith<'a> (fun _ -> true)
    
let rec findParentCachedWith<'a when 'a : null and 'a :> Node> filter meta (node : Node) =
    node
    |> GodotObject.tryGetMetaAs<'a> meta
    |> Option.orElseWith (fun () ->
        let result =
            match node |> Node.tryGetParent<'a> with
            | None -> None
            | Some p when filter p -> Some p
            | Some p -> p |> findParentCachedWith<'a> filter meta
        if result |> Option.isSome then
            node |> GodotObject.setMeta meta result.Value
        result
    )
        
let findParentCached<'a when 'a : null and 'a :> Node> meta (node : Node) =
    node |> findParentCachedWith<'a> (fun _ -> true) meta

let private setChildrenCacheMonitor prefix meta (root : Node) (node : Node) =
    let signal = new StringName $"_{prefix}_{meta}"
    if node |> GodotObject.hasMeta signal |> not then
        let clear () =
            root |> GodotObject.removeMeta meta |> ignore
        
        node |> GodotObject.setMeta signal true
        node.add_ChildEnteredTree (fun _ -> clear())
        node.add_ChildExitingTree (fun _ -> clear())
    
let getChildrenCachedWith<'a when 'a: not struct and 'a :> Node> filter meta (node : Node) =
    let meta = new StringName $"_cached_children_{meta}"
    node
    |> GodotObject.getMetaWithDefaultAsArray<'a> meta (lazy (
        node |> setChildrenCacheMonitor "cached_signal" meta node
        node |> Node.getChildrenWith<'a> filter |> Collections.Array<'a>
    ))
    
let getChildrenCached<'a when 'a: not struct and 'a :> Node> meta (node : Node) =
    node |> getChildrenCachedWith<'a> (fun _ -> true) meta
    
let getChildrenRecCachedWith<'a when 'a: not struct and 'a :> Node> filter meta (node : Node) =
    let meta = new StringName $"_cached_rec_children_{meta}"
    node
    |> GodotObject.getMetaWithDefaultAsArray<'a> meta (lazy (
        node |> setChildrenCacheMonitor "cached_signal" meta node
        node
        |> Node.getChildrenRec
        |> Seq.choose (fun (child: Node) ->
            child |> setChildrenCacheMonitor "cached_child_signal" meta node
            match child with
            | :? 'a as a when filter a -> Some a
            | _ -> None
        )
        |> Collections.Array<'a>
    ))
    
let getChildrenRecCached<'a when 'a: not struct and 'a :> Node> meta (node : Node) =
    node |> getChildrenRecCachedWith<'a> (fun _ -> true) meta