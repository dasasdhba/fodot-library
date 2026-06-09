module FSharp.Builder

type MaybeBuilder() =
    member this.Bind(x, f) = x |> Option.bind f
    member this.Return(x) = Some x
    member this.ReturnFrom(x) = x
    member this.Zero() = None

let maybe = MaybeBuilder()

type ResultBuilder() =
    member this.Bind(x, f) = x |> Result.bind f
    member this.Return(x) = Ok x
    member this.ReturnFrom(x) = x

let result = ResultBuilder()