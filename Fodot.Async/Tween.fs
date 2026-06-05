module Fodot.Async.Tween

open Godot

let asTask (tween: Tween) = task {
    do! tween.ToSignalFinished()
}

let asTaskWith ct (tween: Tween) =
    tween |> Signal.awaitWith<unit> ct Tween.SignalName.Finished