module Fodot.Core.Node

open System
open Fodot.Core
open Fodot.Core.GodotObject
open Godot

// node access

let isAccessSafe (node : Node) =
    (node.IsInsideTree () |> not) || node.IsNodeReady ()

let addChildInternal (child : Node) inter (node : Node) =
    if node |> isAccessSafe then
        node.AddChild(child, false, inter)
    else
        node |> callDeferred "add_child" (child, inter)

let addChild (child : Node) (node : Node) =
    node |> addChildInternal child Node.InternalMode.Disabled
    
let addChildInternalFront (child : Node) (node : Node) =
    node |> addChildInternal child Node.InternalMode.Front

let addChildInternalBack (child : Node) (node : Node) =
    node |> addChildInternal child Node.InternalMode.Back

let addSibling (sibling : Node) (node : Node) =
    if node |> isAccessSafe then
        node.AddSibling(sibling)
    else
        node |> callDeferred "add_sibling" sibling

let moveChild (child: Node) (idx : int) (node : Node) =
    if node |> isAccessSafe then
        node.MoveChild(child, idx)
    else
        node |> callDeferred "move_child" (child, idx)

let removeChild (child : Node) (node : Node) =
    if node |> isAccessSafe then
        node.RemoveChild(child)
    else
        node |> callDeferred "remove_child" child

let reparent parent keep (node : Node) =
    if parent |> isAccessSafe then
        node.Reparent(parent, keep)
    else
        node |> callDeferred "reparent" (parent, keep)

let reparentKeep parent (node : Node) =
    node |> reparent parent true

let reparentDirectly parent (node : Node) =
    node |> reparent parent false

// node get

let getNode<'a when 'a: not struct and 'a : null and 'a :> Node> (name : string) (node : Node) =
    node.GetNode<'a>(name)
    |> Option.ofObj
    |> Option.defaultWith (fun () -> failwith $"{node}: Node {name} not found")

let tryGetNode<'a when 'a: not struct and 'a : null and 'a :> Node> (name : string) (node : Node) =
    match node.GetNodeOrNull<'a> name with
    | null -> None
    | node -> Some node

let getParent<'a when 'a: not struct and 'a : null and 'a :> Node> (node : Node) =
    node.GetParent<'a>()
    |> Option.ofObj
    |> Option.defaultWith (fun () -> failwith $"{node} does not own a parent")

let tryGetParent<'a when 'a: not struct and 'a : null and 'a :> Node> (node : Node) =
    match node.GetParentOrNull<'a>() with
    | null -> None
    | node -> Some node

let private getChildWith<'a when 'a: not struct and 'a : null and 'a :> Node> (idx : int) (inter : bool) (node : Node) =
    node.GetChild<'a>(idx, inter)
    |> Option.ofObj
    |> Option.defaultWith (fun () -> failwith $"{node}: Child at {idx} not found")

let getChild<'a when 'a: not struct and 'a : null and 'a :> Node> (idx : int) (node : Node) =
    node |> getChildWith<'a> idx false

let getChildInternal<'a when 'a: not struct and 'a : null and 'a :> Node> (idx : int) (node : Node) =
    node |> getChildWith<'a> idx true

let private tryGetChildWith<'a when 'a: not struct and 'a : null and 'a :> Node> (idx: int) inter (node : Node) =
    match node.GetChildOrNull<'a>(idx, inter) with
    | null -> None
    | node -> Some node

let tryGetChild<'a when 'a: not struct and 'a : null and 'a :> Node> (idx : int) (node : Node) =
    node |> tryGetChildWith<'a> idx false

let tryGetChildInternal<'a when 'a: not struct and 'a : null and 'a :> Node> (idx : int) (node : Node) =
    node |> tryGetChildWith<'a> idx true

let getChildren<'a when 'a: not struct and 'a :> Node> (node : Node) =
    node.GetChildren ()
    |> List.ofSeq
    |> List.choose (fun c ->
        match c with
        | :? 'a as a -> Some a
        | _ -> None
    )

let rec getChildrenRecWith<'a when 'a: not struct and 'a :> Node> filter (node: Node) =
    node
    |> getChildren<'a>
    |> List.fold (fun acc child ->
        let acc = if filter child then acc @ [child] else acc
        acc |> List.append (child |> getChildrenRecWith filter)
    ) []

let getChildrenRec<'a when 'a: not struct and 'a :> Node> (node: Node) =
    node |> getChildrenRecWith (fun _ -> true)
    
let getChildrenAndSelfRecWith<'a when 'a: not struct and 'a :> Node> filter (node: Node) =
    let children = node |> getChildrenRecWith filter
    if filter node then
        node :: children
    else
        children

let getChildrenAndSelfRec<'a when 'a: not struct and 'a :> Node> (node: Node) =
    node |> getChildrenAndSelfRecWith (fun _ -> true)

// bridge event

let bindChild (child : Node) (node : Node) =
    child.add_TreeEntered (fun _ ->
        if child.GetParent () <> node then
            child.SetBlockSignals true
            child.QueueFree ()
    )

let createEventBy<'n, 'a when 'n :> Node> (child : 'n) signal (node: Node) =
    let event = Event<'a>()
    signal event.Trigger
    
    node |> bindChild child
    node |> addChildInternalFront child
    
    event.Publish

let createDeleteEvent (node: Node) =
    let child = new GodotBridge.DeleteBridge()
    node |> createEventBy child child.add_SignalDeleted

let createInputEvent (node: Node) =
    let child = new GodotBridge.InputBridge()
    node |> createEventBy child child.add_SignalInput

let createUnhandledInputEvent (node: Node) =
    let child = new GodotBridge.UnhandledInputBridge()
    node |> createEventBy child child.add_SignalUnhandledInput

type private CachedEvent () =
    inherit RefCounted ()
    
    member val DeleteEvent : IEvent<unit> option = None with get, set
    member val InputEvent : IEvent<InputEvent> option = None with get, set
    member val UnhandledInputEvent : IEvent<InputEvent> option = None with get, set
    
let private getCachedEventWith getter setter creator node =
    let cache = node |> getMetaWithDefaultAs "_fs_node_cached_event" (lazy new CachedEvent())
    match getter cache with
    | Some event -> event
    | None ->
        let event = creator ()
        setter cache event
        event

let getDeleteEvent (node: Node) =
    node
    |> getCachedEventWith
        _.DeleteEvent
        (fun cache event -> cache.DeleteEvent <- Some event)
        (fun () -> node |> createDeleteEvent)

let getInputEvent (node: Node) =
    node
    |> getCachedEventWith
        _.InputEvent
        (fun cache event -> cache.InputEvent <- Some event)
        (fun () -> node |> createInputEvent)

let getUnhandledInputEvent (node: Node) =
    node
    |> getCachedEventWith
        _.UnhandledInputEvent
        (fun cache event -> cache.UnhandledInputEvent <- Some event)
        (fun () -> node |> createUnhandledInputEvent)

// dispose

let bindDisposable (dispose : IDisposable) (node: Node) =
    let del = node |> getDeleteEvent
    del.Add (fun () -> dispose.Dispose ())

let bindNode (another : Node) (node: Node) =
    let del = node |> getDeleteEvent
    del.Add (fun () -> another.QueueFree ())

// init
    
let initScripts (node: Node) =
    node |> FScript.init
    node |> getChildrenRec |> List.iter (fun c -> c |> FScript.init)