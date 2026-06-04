module Fodot.Async.GD

open Fodot.Core

type GDSignal<'a> with
    member this.AsVariantTask () =
        this.Object |> GodotObject.toSignal this.SignalName
        
    member this.AsVariantTaskWith ct =
        this.Object |> GodotObject.toSignalWith ct this.SignalName
        
    member this.AsTaskWith ct =
        this |> Event.awaitWith ct
        
    member this.AsTask () =
        this |> Event.await