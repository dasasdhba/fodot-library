module Fodot.Async.GD

open System.Threading
open Fodot.Core

type GDSignal<'a> with
    member this.AsTask ?cancellationToken =
        this |> Event.awaitWith (defaultArg cancellationToken CancellationToken.None)