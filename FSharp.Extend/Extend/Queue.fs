module FSharp.Extend.Queue

open System.Collections.Generic

let tryPeek (queue: Queue<'a>) =
    match queue.TryPeek() with
    | true, x -> Some x
    | _ -> None

let tryDequeue (queue: Queue<'a>) =
    match queue.TryDequeue() with
    | true, x -> Some x
    | _ -> None