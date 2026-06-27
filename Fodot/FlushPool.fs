namespace Fodot

open System.Collections.Concurrent
open System.Collections.Generic
open Godot

type SortedFlushPool<'t, 'a>(getter: KeyValuePair<'t, 'a> -> Node, ?comparer : IComparer<'t>) =
    let items =
        match comparer with
        | None -> SortedList<'t, 'a>()
        | Some c -> SortedList<'t, 'a>(c)
        
    let pending = ConcurrentQueue<SortedList<'t, 'a> -> unit>()
    
    member this.Flush() =
        let mutable op = Unchecked.defaultof<_>
        while pending.TryDequeue(&op) do
            op items
    
    member this.Iter() =
        items
        |> Seq.filter (getter >> (fun n ->
            GodotObject.IsInstanceValid n && n.IsInsideTree()
        ))
    
    member this.IterKeys() =
        this.Iter()
        |> Seq.map _.Key
    
    member this.IterValues() =
        this.Iter()
        |> Seq.map _.Value
        
    member this.QueueAdd (key : 't, x: 'a) =
        pending.Enqueue(_.Add(key, x))
        
    member this.QueueRemove (key : 't) =
        pending.Enqueue(_.Remove(key) >> ignore)
        
    /// One should make sure this is called once only
    member this.Track (key : 't, x: 'a) =
        let node = getter (KeyValuePair(key, x))
        if node.IsInsideTree() then
            this.QueueAdd(key, x)

        node.add_TreeEntered (fun _ -> this.QueueAdd (key, x))
        node.add_TreeExited (fun _ -> this.QueueRemove key)
        
    member this.Clear() =
        pending.Enqueue(_.Clear())

type SortedFlushNodes<'t, 'a when 't :> Node>(?comparer : IComparer<'t>) =
    inherit SortedFlushPool<'t, 'a>(_.Key, ?comparer = comparer)

type SortedFlushIdleNodes<'t, 'a when 't :> Node>() =
    inherit SortedFlushNodes<'t, 'a>(comparer = (Node.idleComparer :?> IComparer<'t>))

type SortedFlushPhysicsNodes<'t, 'a when 't :> Node>() =
    inherit SortedFlushNodes<'t, 'a>(comparer = (Node.physicsComparer :?> IComparer<'t>))

type FlushPool<'a>(getter : 'a -> Node) =
    let items = ResizeArray<'a>()
    let pending = ConcurrentQueue<ResizeArray<'a> -> unit>()
    
    member this.Flush() =
        let mutable op = Unchecked.defaultof<_>
        while pending.TryDequeue(&op) do
            op items

    member this.Iter() =
        items
        |> Seq.filter (getter >> (fun n ->
            GodotObject.IsInstanceValid n && n.IsInsideTree()
        ))
    
    member this.QueueAdd (x: 'a) =
        pending.Enqueue(_.Add(x))
    
    member this.QueueRemove (x: 'a) =
        pending.Enqueue(_.Remove(x) >> ignore)
    
    /// One should make sure this is called once only
    member this.Track(x: 'a) =
        let node = getter x
        if node.IsInsideTree() then
            this.QueueAdd x

        node.add_TreeEntered (fun _ -> this.QueueAdd x)
        node.add_TreeExited (fun _ -> this.QueueRemove x)
    
    member this.Clear() =
        pending.Enqueue(_.Clear())
        
type FlushNodes<'a when 'a :> Node>() =
    inherit FlushPool<'a>(fun a -> a :> Node)