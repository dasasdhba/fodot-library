module Moon.Module.Action

open System
open Godot
open Fodot

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
