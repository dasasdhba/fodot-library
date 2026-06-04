module FSharp.Concurrent.Stack

open System.Collections.Concurrent

let tryPeek (stack: ConcurrentStack<'a>) =
    match stack.TryPeek() with
    | true, obj -> Some obj
    | _ -> None
    
let tryPop (stack: ConcurrentStack<'a>) =
    match stack.TryPop() with
    | true, obj -> Some obj
    | _ -> None