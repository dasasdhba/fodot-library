module FSharp.Option

let asObj (x: 'a option) =
    match x with
    | Some x -> x
    | None -> null