[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Fodot.Async.Tween

open FSharp.Threading
open Godot

let asTask (tween: Tween) =
    tween.ToSignalFinished() |> Task.asUnit

let asTaskWith ct (tween: Tween) =
    tween |> Signal.awaitWith<unit> ct Tween.SignalName.Finished