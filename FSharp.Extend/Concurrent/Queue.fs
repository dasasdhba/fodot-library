module FSharp.Concurrent.Queue

open System.Collections.Concurrent

let tryPeek (queue: ConcurrentQueue<'a>) =
    match queue.TryPeek() with
    | true, x -> Some x
    | _ -> None

let tryDequeue (queue: ConcurrentQueue<'a>) =
    match queue.TryDequeue() with
    | true, x -> Some x
    | _ -> None