[<AutoOpen>]
module Moon.GDAsync

open System.Threading
open FSharp
open Moon

type GDSignal<'a> with
    member this.AsTask ?cancellationToken =
        this |> Event.awaitWith (defaultArg cancellationToken CancellationToken.None)
