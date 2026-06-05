module Fodot.Core.Node

open System
open Fodot.Bridge
open Fodot.Core
open Godot

// node access

let isAccessSafe (node : Node) =
    (node.IsInsideTree () |> not) || (node.IsNodeReady () && GodotThread.IsMainThread())

let addChildInternal (child : Node) inter (node : Node) =
    if node |> isAccessSafe then
        node.AddChild(child, false, inter)
    else
        node |> GDTask.post _.AddChild(child, false, inter)

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
        node |> GDTask.post _.AddSibling(sibling)

let moveChild (child: Node) (idx : int) (node : Node) =
    if node |> isAccessSafe then
        node.MoveChild(child, idx)
    else
        node |> GDTask.post _.MoveChild(child, idx)

let removeChild (child : Node) (node : Node) =
    if node |> isAccessSafe then
        node.RemoveChild(child)
    else
        node |> GDTask.post _.RemoveChild(child)

// node get

let getNode<'a when 'a: not struct and 'a :> Node> (path : NodePath) (node : Node) =
    node.GetNode<'a>(path)

let tryGetNode<'a when 'a: null and 'a :> Node> (path : NodePath) (node : Node) =
    node.GetNodeOrNull<'a> path |> Option.ofObj

let getParent<'a when 'a: not struct and 'a :> Node> (node : Node) =
    node.GetParent<'a>()

let tryGetParent<'a when 'a : null and 'a :> Node> (node : Node) =
    node.GetParentOrNull<'a>() |> Option.ofObj

let getChild<'a when 'a: not struct and 'a :> Node> (idx : int) (node : Node) =
    node.GetChild<'a>(idx)

let getChildInternal<'a when 'a: not struct and 'a :> Node> (idx : int) (node : Node) =
    node.GetChild<'a>(idx, true)

let tryGetChildInternalOrNot<'a when 'a : null and 'a :> Node> (idx : int) inter (node : Node) =
    node.GetChildOrNull<'a>(idx, inter) |> Option.ofObj

let tryGetChild<'a when 'a : null and 'a :> Node> (idx : int) (node : Node) =
    node.GetChildOrNull<'a>(idx) |> Option.ofObj

let tryGetChildInternal<'a when 'a : null and 'a :> Node> (idx : int) (node : Node) =
    node.GetChildOrNull<'a>(idx, true) |> Option.ofObj

let getChildrenInternalOrNot<'a when 'a: not struct and 'a :> Node> inter (node : Node) = seq {
    for n in node.GetChildren inter do
        match n with
        | :? 'a as a -> yield a
        | _ -> ()
}

let getChildren<'a when 'a: not struct and 'a :> Node> (node : Node) =
    node |> getChildrenInternalOrNot<'a> false

let getChildrenInternal<'a when 'a: not struct and 'a :> Node> (node : Node) =
    node |> getChildrenInternalOrNot<'a> true

let rec getChildrenRecInternalOrNot<'a when 'a: not struct and 'a :> Node> inter (node: Node) = seq {
    for n in node |> getChildrenInternalOrNot<'a> inter do
        yield n
        yield! n |> getChildrenRecInternalOrNot<'a> inter
}

let getChildrenRec<'a when 'a: not struct and 'a :> Node> (node: Node) =
    node |> getChildrenRecInternalOrNot<'a> false

let getChildrenRecInternal<'a when 'a: not struct and 'a :> Node> (node: Node) =
    node |> getChildrenRecInternalOrNot<'a> true

// reparent

let reparent parent keep (node : Node) =
    let pSafe =
        node
        |> tryGetParent
        |> Option.map (fun p -> p |> isAccessSafe)
        |> Option.defaultValue true
    
    if pSafe && parent |> isAccessSafe then
        node.Reparent(parent, keep)
    else
        node |> GDTask.post _.Reparent(parent, keep)

let reparentKeep parent (node : Node) =
    node |> reparent parent true

let reparentDirectly parent (node : Node) =
    node |> reparent parent false

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
    let child = new DeleteBridge()
    node |> createEventBy child child.add_Deleted

let createInputEvent (node: Node) =
    let child = new InputBridge()
    node |> createEventBy child child.add_Input

let createUnhandledInputEvent (node: Node) =
    let child = new UnhandledInputBridge()
    node |> createEventBy child child.add_UnhandledInput

type private CachedEvent () =
    member val DeleteEvent : IEvent<unit> option = None with get, set
    member val InputEvent : IEvent<InputEvent> option = None with get, set
    member val UnhandledInputEvent : IEvent<InputEvent> option = None with get, set

let private cachedTable = WeakMeta<CachedEvent>()
    
let private getCachedEventWith getter setter creator node =
    let cache = cachedTable |> WeakMeta.getOrAdd node (lazy CachedEvent ())
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
    seq {
        yield node
        yield! node |> getChildrenRecInternal
    }
    |> Seq.rev
    |> Seq.iter (fun c -> c |> FScript.init)
    
let duplicateWith (flag : Node.DuplicateFlags) (node: Node) =
    let result = node.Duplicate(int flag)
    result |> initScripts
    result
    
let duplicate (node: Node) =
    node |> duplicateWith Node.DuplicateFlags.Default