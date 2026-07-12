namespace Fodot

open System.Collections.Concurrent
open System.Collections.Generic
open Godot

type SortedFlushPool<'a>(getter: 'a -> Node, ?comparer : IComparer<Node>) =
    let items = ResizeArray<'a>()
    let pending = ConcurrentQueue<ResizeArray<'a> -> unit>()
    let comparer =
        comparer
        |> Option.map (fun c ->
            {
                new IComparer<'a> with
                member this.Compare (x, y) =
                    c.Compare(getter x, getter y)
            }
        )
    
    member this.Flush() =
        items.RemoveAll (fun x ->
            let n = getter x
            (GodotObject.IsInstanceValid n && n.IsInsideTree()) |> not
        ) |> ignore
        
        let mutable ad = Unchecked.defaultof<_>
        while pending.TryDequeue(&ad) do
            ad items
    
    member this.Iter() : 'a seq =
        items
        |> Seq.filter (getter >> (fun n ->
            GodotObject.IsInstanceValid n && n.IsInsideTree()
        ))
    
    member private this.QueueAdd (x: 'a) =
        pending.Enqueue(fun _ ->
            let n = getter x
            if (GodotObject.IsInstanceValid n && n.IsInsideTree()) |> not then () else
            
            let index =
                match comparer with
                | Some c -> items.BinarySearch(x, c)
                | None -> items.BinarySearch(x)
            let index =
                if index < 0 then ~~~index else index
            items.Insert(index, x)
        )
        
    member private this.QueueRemove (x: 'a) =
        pending.Enqueue(fun _ -> items.Remove x |> ignore)
    
    /// One should make sure this is called once only
    member this.Track(x: 'a) =
        let node = getter x
        if node.IsInsideTree() then
            this.QueueAdd x

        node.add_TreeEntered (fun _ -> this.QueueAdd x)
        node.add_TreeExited (fun _ -> this.QueueRemove x)

type SortedFlushIdlePool<'a>(getter) =
    inherit SortedFlushPool<'a>(getter, Node.idleComparer)

type SortedFlushPhysicsPool<'a>(getter) =
    inherit SortedFlushPool<'a>(getter, Node.physicsComparer)

type SortedFlushIdleNodes<'a when 'a :> Node>() =
    inherit SortedFlushIdlePool<'a>(fun a -> a :> Node)

type SortedFlushPhysicsNodes<'a when 'a :> Node>() =
    inherit SortedFlushPhysicsPool<'a>(fun a -> a :> Node)

type FlushPool<'a>(getter : 'a -> Node) =
    let items = ResizeArray<'a>()
    let pending = ConcurrentQueue<ResizeArray<'a> -> unit>()
    
    member this.Flush() =
        let mutable op = Unchecked.defaultof<_>
        while pending.TryDequeue(&op) do
            op items

    member this.Iter() = seq {
        for i = items.Count - 1 downto 0 do
            let item = items[i]
            let n = getter item
            if GodotObject.IsInstanceValid n && n.IsInsideTree() then
                yield item
            else
                items.RemoveAt i
    }
    
    member private this.QueueAdd (x: 'a) =
        pending.Enqueue(fun _ ->
            let n = getter x
            if GodotObject.IsInstanceValid n && n.IsInsideTree() then
                items.Add x
        )

    member private this.QueueRemove (x: 'a) =
        pending.Enqueue(fun _ -> items.Remove x |> ignore)
    
    /// One should make sure this is called once only
    member this.Track(x: 'a) =
        let node = getter x
        if node.IsInsideTree() then
            this.QueueAdd x

        node.add_TreeEntered (fun _ -> this.QueueAdd x)
        node.add_TreeExited (fun _ -> this.QueueRemove x)
        
type FlushNodes<'a when 'a :> Node>() =
    inherit FlushPool<'a>(fun a -> a :> Node)