module Fodot.Extend.Node

open System.Collections.Generic
open FSharp.Extend
open Godot
open Fodot.Core

let getOwnerOrSelf (node : Node) =
    node.Owner
    |> Option.ofObj
    |> Option.defaultValue node

let getNameInOwner (node : Node) =
    if node.IsUniqueNameInOwner () then
        "%" + node.Name.ToString ()
    else
        let owner = node |> getOwnerOrSelf
        owner.GetPathTo node |> string

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

let rec chooseParent predictor (node : Node) =
    node
    |> Node.tryGetParent<Node>
    |> Option.bind (fun p ->
        match predictor p with
        | None -> p |> chooseParent predictor
        | v -> v
    )

let findParentWith<'a when 'a : null and 'a :> Node> filter (node : Node) =
    node |> chooseParent (fun p ->
        match p with
        | :? 'a as a when filter a -> Some a
        | _ -> None
    )
    
let findParent<'a when 'a : null and 'a :> Node> (node : Node) =
    node |> findParentWith<'a> (fun _ -> true)

type ParentCache<'a> = WeakMeta<ReLazy<'a option>>

let rec chooseParentCached (map: ParentCache<'a>) predictor (node : Node) =
    map
    |> WeakMeta.tryGet node
    |> Option.bind _.Value
    |> Option.orElseWith (fun () ->
        let result = ReLazy (fun _ ->
            node
            |> Node.tryGetParent<Node>
            |> Option.bind (fun p ->
                match predictor p with
                | None -> p |> chooseParentCached map predictor
                | v -> v
            )
        )
        node.add_TreeExited (fun _ -> result.Rebuild())
        map |> WeakMeta.update node result
        result.Value
    )

let findParentCachedWith<'a when 'a : null and 'a :> Node> (map: ParentCache<'a>) filter (node : Node) =
    node |> chooseParentCached map (fun p ->
        match p with
        | :? 'a as a when filter a -> Some a
        | _ -> None
    )
        
let findParentCached<'a when 'a : null and 'a :> Node> map (node : Node) =
    node |> findParentCachedWith<'a> map (fun _ -> true)

// children

let private setChildrenCacheMonitor (re : ReLazy<'a>) (node : Node) =
    node.add_ChildEnteredTree (fun _ -> re.Rebuild())
    node.add_ChildExitingTree (fun _ -> re.Rebuild())

type ChildrenCache<'a> = WeakMeta<ReLazy<'a list>>

let chooseChildrenCachedInternalOrNot (map : ChildrenCache<'a>) inter predictor (node : Node) =
    map
    |> WeakMeta.tryGet node
    |> Option.map _.Value
    |> Option.defaultWith (fun () ->
        let re = ReLazy (fun _ ->
            node
            |> Node.getChildrenInternalOrNot<Node> inter
            |> Seq.choose predictor
            |> List.ofSeq
        )
        node.add_TreeExited (fun _ -> re.Rebuild())
        node |> setChildrenCacheMonitor re
        map |> WeakMeta.update node re
        re.Value
    )

let chooseChildrenCachedInternal map predictor node =
    node |> chooseChildrenCachedInternalOrNot map true predictor

let chooseChildrenCached map predictor node =
    node |> chooseChildrenCachedInternalOrNot map false predictor

let getChildrenCachedInternalOrNotWith<'a when 'a: not struct and 'a :> Node> map inter filter (node : Node) =
    node |> chooseChildrenCachedInternalOrNot map inter (fun c ->
        match c with
        | :? 'a as a when filter a -> Some a
        | _ -> None
    )

let getChildrenCachedWith<'a when 'a: not struct and 'a :> Node> map filter (node : Node) =
    node |> getChildrenCachedInternalOrNotWith<'a> map false filter

let getChildrenInternalCachedWith<'a when 'a: not struct and 'a :> Node> map filter (node : Node) =
    node |> getChildrenCachedInternalOrNotWith<'a> map true filter
    
let getChildrenCached<'a when 'a: not struct and 'a :> Node> map (node : Node) =
    node |> getChildrenCachedWith<'a> map (fun _ -> true)

let getChildrenInternalCached<'a when 'a: not struct and 'a :> Node> map (node : Node) =
    node |> getChildrenInternalCachedWith<'a> map (fun _ -> true)

type ChildrenRecCache<'a> = WeakMeta<ReLazy<'a list * HashSet<Node>>>

let chooseChildrenRecCachedInternalOrNot (map : ChildrenRecCache<'a>) inter predictor (node : Node) =
    map
    |> WeakMeta.tryGet node
    |> Option.map (fun r -> r.Value |> fst)
    |> Option.defaultWith (fun () ->
        let re = ReLazy ()
        node.add_TreeExited (fun _ -> re.Rebuild())
        node |> setChildrenCacheMonitor re
        
        re.Build (fun _ ->
            let last =
                map
                |> WeakMeta.tryGet node
                |> Option.map (fun r -> r.Value |> snd)
                |> Option.defaultWith (fun () -> HashSet<Node>())
            
            let nodes =
                node
                |> Node.getChildrenRecInternalOrNot<Node> inter
            
            let list =
                nodes
                |> Seq.choose (fun c ->
                    if last.Contains c |> not then
                        c |> setChildrenCacheMonitor re
                    c |> predictor
                )
                |> List.ofSeq
                
            nodes |> Seq.iter (fun n -> last.Add n |> ignore)
            list, last
        )
        
        map |> WeakMeta.update node re
        re.Value |> fst
    )
    
let chooseChildrenRecCachedInternal map predictor node =
    node |> chooseChildrenRecCachedInternalOrNot map true predictor
    
let chooseChildrenRecCached map predictor node =
    node |> chooseChildrenRecCachedInternalOrNot map false predictor
    
let getChildrenRecCachedInternalOrNotWith<'a when 'a: not struct and 'a :> Node> map inter filter (node : Node) =
    node |> chooseChildrenRecCachedInternalOrNot map inter (fun c ->
        match c with
        | :? 'a as a when filter a -> Some a
        | _ -> None
    )

let getChildrenRecCachedWith<'a when 'a: not struct and 'a :> Node> map filter (node : Node) =
    node |> getChildrenRecCachedInternalOrNotWith<'a> map false filter

let getChildrenRecInternalCachedWith<'a when 'a: not struct and 'a :> Node> map filter (node : Node) =
    node |> getChildrenRecCachedInternalOrNotWith<'a> map true filter
    
let getChildrenRecCached<'a when 'a: not struct and 'a :> Node> map (node : Node) =
    node |> getChildrenRecCachedWith<'a> map (fun _ -> true)

let getChildrenRecInternalCached<'a when 'a: not struct and 'a :> Node> map (node : Node) =
    node |> getChildrenRecInternalCachedWith<'a> map (fun _ -> true)
    
// fscripts

let getNodeFs<'a> path (node : Node) =
    node |> Node.getNode path |> FScript.get<'a>

let tryGetNodeFs<'a> path (node : Node) =
    node |> Node.tryGetNode path |> Option.bind (fun n -> n |> FScript.tryGet<'a>)

let getParentFs<'a> (node : Node) =
    node |> Node.getParent |> FScript.get<'a>

let tryGetParentFs<'a> (node : Node) =
    node |> Node.tryGetParent |> Option.bind (fun p -> p |> FScript.tryGet<'a>)

let getChildFsInternalOrNot idx inter (node : Node) =
    node.GetChild(idx, inter) |> FScript.get<'a>

let getChildFs<'a> idx (node : Node) =
    node |> Node.getChild idx |> FScript.get<'a>

let getChildInternalFs<'a> idx (node : Node) =
    node |> Node.getChildInternal idx |> FScript.get<'a>

let tryGetChildFsInternalOrNot idx inter (node : Node) =
    node |> Node.tryGetChildInternalOrNot idx inter |> Option.bind (fun c -> c |> FScript.tryGet<'a>)

let tryGetChildFs<'a> idx (node : Node) =
    node |> Node.tryGetChild idx |> Option.bind (fun c -> c |> FScript.tryGet<'a>)

let tryGetChildInternalFs<'a> idx (node : Node) =
    node |> Node.tryGetChildInternal idx |> Option.bind (fun c -> c |> FScript.tryGet<'a>)

let findParentFs<'a> (node : Node) =
    node
    |> chooseParent (fun p -> p |> FScript.tryGet<'a>)

let findParentFsCached<'a> map (node : Node) =
    node
    |> chooseParentCached map (fun p -> p |> FScript.tryGet<'a>)

let getChildrenFsInternalOrNot<'a> inter (node : Node) =
    node
    |> Node.getChildrenInternalOrNot inter
    |> Seq.choose (fun c -> c |> FScript.tryGet<'a>)

let getChildrenFs<'a> (node : Node) =
    node |> getChildrenFsInternalOrNot<'a> false

let getChildrenInternalFs<'a> (node : Node) =
    node |> getChildrenFsInternalOrNot<'a> true

let getChildrenFsRecInternalOrNot<'a> inter (node : Node) =
    node
    |> Node.getChildrenRecInternalOrNot inter
    |> Seq.choose (fun c -> c |> FScript.tryGet<'a>)

let getChildrenFsRec<'a> (node : Node) =
    node |> getChildrenFsRecInternalOrNot<'a> false
    
let getChildrenInternalFsRec<'a> (node : Node) =
    node |> getChildrenFsRecInternalOrNot<'a> true
    
let getChildrenFsCachedInternalOrNot<'a> map inter (node : Node) =
    node
    |> chooseChildrenCachedInternalOrNot map inter (fun c -> c |> FScript.tryGet<'a>)

let getChildrenFsCached<'a> map (node : Node) =
    node |> getChildrenFsCachedInternalOrNot<'a> map false

let getChildrenInternalFsCached<'a> map (node : Node) =
    node |> getChildrenFsCachedInternalOrNot<'a> map true

let getChildrenFsRecCachedInternalOrNot<'a> map inter (node : Node) =
    node
    |> chooseChildrenRecCachedInternalOrNot map inter (fun c -> c |> FScript.tryGet<'a>)

let getChildrenFsRecCached<'a> map (node : Node) =
    node |> getChildrenFsRecCachedInternalOrNot<'a> map false
    
let getChildrenInternalFsRecCached<'a> map (node : Node) =
    node |> getChildrenFsRecCachedInternalOrNot<'a> map true
