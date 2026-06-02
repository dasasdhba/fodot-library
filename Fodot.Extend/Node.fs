module Fodot.Extend.Node

open Godot
open Fodot.Core

let getOwnerOrSelf (node : Node) =
    node.Owner
    |> Option.ofObj
    |> Option.defaultValue node

// relative loading

let rec getSceneFilePath (node : Node) =
    if node.SceneFilePath <> "" then
        Some node.SceneFilePath
    else
        match node.Owner |> Option.ofObj with
        | None -> None
        | Some owner -> owner |> getSceneFilePath

let asRelativePath (path : string) (node : Node) =
    match node |> getSceneFilePath with
    | None -> path
    | Some p ->
        match path with
        | s when s.StartsWith "@" ->
            let body = s[1..]
            let name = p.GetFile().GetBaseName ()
           
            let idx = name.LastIndexOf '_'
            let name = if idx < 0 then name else name[..(idx - 1)]
            
            $"{p.GetBaseDir()}/{name}_{body}"
        | "" ->
            p
        | _ ->
            $"{p.GetBaseDir()}/{path}"

let load (path : string) (node : Node) =
    GD.load (node |> asRelativePath path)

let loadAs<'a when 'a :> Resource> path (node : Node) =
    GD.loadAs<'a> (node |> asRelativePath path)

let tryLoad path node =
    GD.tryLoad (node |> asRelativePath path)

let tryLoadAs<'a when 'a :> Resource> path (node : Node) =
    GD.tryLoadAs<'a> (node |> asRelativePath path)

// parent

let rec findParentWith<'a when 'a : null and 'a :> Node> filter (node : Node) =
    match node.GetParentOrNull<Node>() with
    | null -> None
    | :? 'a as a when filter a -> Some a
    | p -> p |> findParentWith<'a> filter
    
let findParent<'a when 'a : null and 'a :> Node> (node : Node) =
    node |> findParentWith<'a> (fun _ -> true)
    
let rec findParentCachedWith<'a when 'a : null and 'a :> Node> filter meta (node : Node) =
    node
    |> GodotObject.tryGetMetaAs<'a> meta
    |> Option.orElseWith (fun () ->
        let result =
            match node.GetParentOrNull<Node>() with
            | null -> None
            | :? 'a as a when filter a -> Some a
            | p -> p |> findParentCachedWith<'a> filter meta
        if result |> Option.isSome then
            node |> GodotObject.setMeta meta result.Value
        result
    )
        
let findParentCached<'a when 'a : null and 'a :> Node> meta (node : Node) =
    node |> findParentCachedWith<'a> (fun _ -> true) meta

// children

let private setChildrenCacheMonitor prefix meta (root : Node) (node : Node) =
    let signal = new StringName $"_{prefix}_{meta}"
    if node |> GodotObject.hasMeta signal |> not then
        let clear () =
            root |> GodotObject.removeMeta meta |> ignore
        
        // this prevent PackedScene.pack
        // in which case the metadata will be null
        // so that signal can be reconstructed here
        node |> GodotObject.setMeta signal (new RefCounted())
        node.add_ChildEnteredTree (fun _ -> clear())
        node.add_ChildExitingTree (fun _ -> clear())
    
let getChildrenCachedInternalOrNotWith<'a when 'a: not struct and 'a :> Node> filter (meta: string) inter (node : Node) =
    let meta = new StringName $"_cached_children_{meta}"
    node
    |> GodotObject.getMetaWithDefaultAsArray<'a> meta (lazy (
        node |> setChildrenCacheMonitor "cached_signal" meta node
        node
        |> Node.getChildrenInternalOrNot<'a> inter
        |> Seq.filter filter
        |> Collections.Array<'a>
    ))

let getChildrenCachedWith<'a when 'a: not struct and 'a :> Node> filter meta (node : Node) =
    node |> getChildrenCachedInternalOrNotWith<'a> filter meta false

let getChildrenInternalCachedWith<'a when 'a: not struct and 'a :> Node> filter meta (node : Node) =
    node |> getChildrenCachedInternalOrNotWith<'a> filter meta true
    
let getChildrenCached<'a when 'a: not struct and 'a :> Node> meta (node : Node) =
    node |> getChildrenCachedWith<'a> (fun _ -> true) meta

let getChildrenInternalCached<'a when 'a: not struct and 'a :> Node> meta (node : Node) =
    node |> getChildrenInternalCachedWith<'a> (fun _ -> true) meta
    
let getChildrenRecCachedInternalOrNotWith<'a when 'a: not struct and 'a :> Node> filter (meta: string) inter (node : Node) =
    let meta = new StringName $"_cached_rec_children_{meta}"
    node
    |> GodotObject.getMetaWithDefaultAsArray<'a> meta (lazy (
        node |> setChildrenCacheMonitor "cached_signal" meta node
        node
        |> Node.getChildrenRecInternalOrNot inter
        |> Seq.choose (fun (child: Node) ->
            child |> setChildrenCacheMonitor "cached_child_signal" meta node
            match child with
            | :? 'a as a when filter a -> Some a
            | _ -> None
        )
        |> Collections.Array<'a>
    ))

let getChildrenRecCachedWith<'a when 'a: not struct and 'a :> Node> filter meta (node : Node) =
    node |> getChildrenRecCachedInternalOrNotWith<'a> filter meta false

let getChildrenRecInternalCachedWith<'a when 'a: not struct and 'a :> Node> filter meta (node : Node) =
    node |> getChildrenRecCachedInternalOrNotWith<'a> filter meta true
    
let getChildrenRecCached<'a when 'a: not struct and 'a :> Node> meta (node : Node) =
    node |> getChildrenRecCachedWith<'a> (fun _ -> true) meta

let getChildrenRecInternalCached<'a when 'a: not struct and 'a :> Node> meta (node : Node) =
    node |> getChildrenRecInternalCachedWith<'a> (fun _ -> true) meta
    
// fscripts

let getNodeFScript<'a> path (node : Node) =
    node |> Node.getNode path |> FScript.get<'a>

let tryGetNodeFScript<'a> path (node : Node) =
    node |> Node.tryGetNode path |> Option.bind (fun n -> n |> FScript.tryGet<'a>)

let getParentFScript<'a> (node : Node) =
    node |> Node.getParent |> FScript.get<'a>

let tryGetParentFScript<'a> (node : Node) =
    node |> Node.tryGetParent |> Option.bind (fun p -> p |> FScript.tryGet<'a>)

let getChildFScriptInternalOrNot idx inter (node : Node) =
    node.GetChild(idx, inter) |> FScript.get<'a>

let getChildFScript<'a> idx (node : Node) =
    node |> Node.getChild idx |> FScript.get<'a>

let getChildInternalFScript<'a> idx (node : Node) =
    node |> Node.getChildInternal idx |> FScript.get<'a>

let tryGetChildFScriptInternalOrNot idx inter (node : Node) =
    node |> Node.tryGetChildInternalOrNot idx inter |> Option.bind (fun c -> c |> FScript.tryGet<'a>)

let tryGetChildFScript<'a> idx (node : Node) =
    node |> Node.tryGetChild idx |> Option.bind (fun c -> c |> FScript.tryGet<'a>)

let tryGetChildInternalFScript<'a> idx (node : Node) =
    node |> Node.tryGetChildInternal idx |> Option.bind (fun c -> c |> FScript.tryGet<'a>)

let findParentFScript<'a> (node : Node) =
    node
    |> findParentWith (fun p -> p |> FScript.contains<'a>)
    |> Option.bind (fun p -> p |> FScript.tryGet<'a>)

let findParentFScriptCached<'a> meta (node : Node) =
    node
    |> findParentCachedWith (fun p -> p |> FScript.contains<'a>) meta
    |> Option.bind (fun p -> p |> FScript.tryGet<'a>)

let getChildrenFScriptsInternalOrNot<'a> inter (node : Node) =
    node
    |> Node.getChildrenInternalOrNot inter
    |> Seq.choose (fun c -> c |> FScript.tryGet<'a>)

let getChildrenFScripts<'a> (node : Node) =
    node |> getChildrenFScriptsInternalOrNot<'a> false

let getChildrenInternalFScripts<'a> (node : Node) =
    node |> getChildrenFScriptsInternalOrNot<'a> true

let getChildrenFScriptsRecInternalOrNot<'a> inter (node : Node) =
    node
    |> Node.getChildrenRecInternalOrNot inter
    |> Seq.choose (fun c -> c |> FScript.tryGet<'a>)

let getChildrenFScriptsRec<'a> (node : Node) =
    node |> getChildrenFScriptsRecInternalOrNot<'a> false
    
let getChildrenInternalFScriptsRec<'a> (node : Node) =
    node |> getChildrenFScriptsRecInternalOrNot<'a> true
    
let getChildrenFScriptsCachedInternalOrNot<'a> meta inter (node : Node) =
    node
    |> getChildrenCachedInternalOrNotWith (fun c -> c |> FScript.contains<'a>) meta inter
    |> Seq.map (fun c -> c |> FScript.get<'a>)

let getChildrenFScriptsCached<'a> meta (node : Node) =
    node |> getChildrenFScriptsCachedInternalOrNot<'a> meta false

let getChildrenInternalFScriptsCached<'a> meta (node : Node) =
    node |> getChildrenFScriptsCachedInternalOrNot<'a> meta true

let getChildrenFScriptsRecCachedInternalOrNot<'a> meta inter (node : Node) =
    node
    |> getChildrenRecCachedInternalOrNotWith (fun c -> c |> FScript.contains<'a>) meta inter
    |> Seq.map (fun c -> c |> FScript.get<'a>)

let getChildrenFScriptsRecCached<'a> meta (node : Node) =
    node |> getChildrenFScriptsRecCachedInternalOrNot<'a> meta false
    
let getChildrenInternalFScriptsRecCached<'a> meta (node : Node) =
    node |> getChildrenFScriptsRecCachedInternalOrNot<'a> meta true