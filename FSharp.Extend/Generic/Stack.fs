module FSharp.Generic.Stack

open System.Collections.Generic

let tryPeek (stack: Stack<'a>) =
    match stack.TryPeek() with
    | true, obj -> Some obj
    | _ -> None
    
let tryPop (stack: Stack<'a>) =
    match stack.TryPop() with
    | true, obj -> Some obj
    | _ -> None