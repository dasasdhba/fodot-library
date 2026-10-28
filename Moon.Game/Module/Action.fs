[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Moon.Action

open System
open Godot

let repeat (interval : float) (physics: bool) (action : unit -> unit)  (node : Node) =
    let mutable timer = 0.0
    node
    |> Engine.addDeltaProcess physics (fun delta ->
        timer <- timer + delta
        if timer >= interval then
            timer <- timer - interval
            action ()
    )

let repeatIdle (interval : float) (action : unit -> unit) (node : Node) =
    repeat interval false action node

let repeatPhysics (interval : float) (action : unit -> unit) (node : Node) =
    repeat interval true action node

let delay (time : float) (physics: bool) (action : unit -> unit) (node : Node) =
    let mutable timer = 0.0
    let mutable proc = Guid.Empty
    proc <-
        node
        |> Engine.addDeltaProcess physics (fun delta ->
            timer <- timer + delta
            if timer >= time then
                action ()
                node |> Engine.removeProcess proc |> ignore
        )
    proc

let delayIdle (time : float) (action : unit -> unit) (node : Node) =
    delay time false action node

let delayPhysics (time : float) (action : unit -> unit) (node : Node) =
    delay time true action node

let until (physics: bool) (check : ProcessFunc<bool>) (node : Node) =
    let mutable proc = Guid.Empty
    proc <-
        node
        |> Engine.addDeltaProcess physics (fun delta ->
            if check.Invoke delta then
                node |> Engine.removeProcess proc |> ignore
        )
    proc

let untilIdle (check : ProcessFunc<bool>) (node : Node) =
    until false check node

let untilPhysics (check : ProcessFunc<bool>) (node : Node) =
    until true check node
