namespace Moon.Library

open System.Threading
open Fodot.Async
open Fodot.Core
open Godot

type SmoothToggle(node : Node, ?flag : bool, ?time : float, ?physics: bool) =
    
    let mutable flag = defaultArg flag false
    let mutable time = defaultArg time 0.5
    let physics = defaultArg physics true
    
    let mutable paused = false
    let mutable value = if flag then 1.0 else 0.0
    
    let mutable bindFunc : (float -> unit) option = None
    
    let updateBind () =
        bindFunc |> Option.iter (fun f -> f value)
    
    let fullyOn = Event<unit>()
    let fullyOff = Event<unit>()
    
    do
        node |> Engine.addDeltaProcess physics (fun delta ->
            if paused then () else
            
            match flag with
            | false when value > 0.0 ->
                value <- Mathf.MoveToward(value, 0.0, delta / time)
                updateBind()
                if value <= 0.0 then
                    fullyOff.Trigger()
            | true when value < 1.0 ->
                value <- Mathf.MoveToward(value, 1.0, delta / time)
                updateBind()
                if value >= 1.0 then
                    fullyOn.Trigger()
            | _ -> ()
            
        ) |> ignore
    
    member this.Flag
        with get() = flag
        and set value = flag <- value
   
    member this.Time
        with get() = time
        and set value = time <- value
    
    member this.Paused
        with get() = paused
        and set value = paused <- value
    
    member this.Value
        with get() = value
        and set v =
            value <- v
            updateBind()
    
    member val FullyOn = fullyOn.Publish
    member val FullyOff = fullyOff.Publish
    
    member this.IsFullyOn () =
        this.Value >= 1.0
        
    member this.IsFullyOff () =
        this.Value <= 0.0
    
    member this.QuickOn () =
        this.Time <- 1.0
        
    member this.QuickOff () =
        this.Time <- 0.0
        
    member this.Bind (f : float -> unit) =
        bindFunc <- Some f
        updateBind()
        
    member this.SwitchOn (?ct : CancellationToken) =
        let ct = defaultArg ct CancellationToken.None
        task {
            if this.IsFullyOn() then
                return ()
            else
                this.Flag <- true
                do! this.FullyOn |> Event.awaitWith ct
        }
        
    member this.SwitchOff (?ct : CancellationToken) =
        let ct = defaultArg ct CancellationToken.None
        task {
            if this.IsFullyOff() then
                return ()
            else
                this.Flag <- false
                do! this.FullyOff |> Event.awaitWith ct
        }