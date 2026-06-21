namespace Moon.Library

open System.Collections.Concurrent
open System.Collections.Generic
open Godot

type FlushPool<'a>(getter : 'a -> Node) =
    let items = HashSet<'a>()
    let pending = ConcurrentQueue<HashSet<'a> -> unit>()
    
    member this.Flush() =
        let mutable op = Unchecked.defaultof<_>
        while pending.TryDequeue(&op) do
            op items

    member this.Iter() =
        items
        |> Seq.filter (getter >> GodotObject.IsInstanceValid)
    
    member this.QueueAdd (x: 'a) =
        pending.Enqueue(fun xs -> xs.Add x |> ignore)
    
    member this.QueueRemove (x: 'a) =
        pending.Enqueue(fun xs -> xs.Remove x |> ignore)
    
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