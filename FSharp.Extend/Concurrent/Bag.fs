module FSharp.Concurrent.Bag

open System.Collections.Concurrent

let tryPeek (bag: ConcurrentBag<'a>) =
    match bag.TryPeek() with
    | true, item -> Some item
    | _ -> None

let tryTake (bag: ConcurrentBag<'a>) =
    match bag.TryTake() with
    | true, item -> Some item
    | _ -> None