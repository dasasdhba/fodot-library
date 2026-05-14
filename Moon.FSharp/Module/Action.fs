module Moon.Module.Action

open System.Threading
open Fodot.Async
open Godot
open Fodot.Core

let repeat (interval : float) (action : unit -> unit) (physics: bool) (node : Node) =
    let mutable timer = 0.0
    node
    |> Engine.addDeltaProcess (fun delta ->
            timer <- timer + delta
            if timer >= interval then
                timer <- timer - interval
                action ()
        )
        physics
        
let repeatIdle (interval : float) (action : unit -> unit) (node : Node) =
    repeat interval action false node
    
let repeatPhysics (interval : float) (action : unit -> unit) (node : Node) =
    repeat interval action true node
    
let delayWith ct (time : float) (action : unit -> unit) (physics: bool) (node : Node) =
    let a = AsyncNode.New node physics ct
    task {
        do! a.Delay time
        action ()
    }
    
let delayIdleWith ct (time : float) (action : unit -> unit) (node : Node) =
    delayWith ct time action false node
    
let delayPhysicsWith ct (time : float) (action : unit -> unit) (node : Node) =
    delayWith ct time action true node
    
let delay (time : float) (action : unit -> unit) (physics: bool) (node : Node) =
    delayWith CancellationToken.None time action physics node
    
let delayIdle (time : float) (action : unit -> unit) (node : Node) =
    delay time action false node
    
let delayPhysics (time : float) (action : unit -> unit) (node : Node) =
    delay time action true node