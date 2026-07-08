namespace Fodot.Async

open System.Threading
open System.Threading.Tasks
open FSharp.Threading
open Fodot
open Godot

type AsyncNode =
    {
        Node : Node
        Physics : bool
        Ct : CancellationToken
    }
    
    static member New (node : Node) physics ct =
        {
            Node = node
            Physics = physics
            Ct = ct
        }
    static member NewIdle (node : Node) ct =
        AsyncNode.New node false ct
    static member NewPhysics (node : Node) ct =
        AsyncNode.New node true ct
        
module AsyncNode =    
    
    let until (predict : ProcessFunc<bool>) (anode : AsyncNode) =
        let event = Event<unit>()
        let proc =
            ProcessConfig.New anode.Physics (Delta (fun delta ->
                if anode.Ct.IsCancellationRequested |> not then
                    if predict.Invoke delta then
                        event.Trigger()
            ))
        let id = proc.AddWith anode.Node
        
        task {
            try
                do! event.Publish |> Event.awaitWith anode.Ct
            finally
                anode.Node |> Engine.removeProcess id |> ignore
        }
        
    let toProcessThread (anode : AsyncNode) =
        anode |> until (Unit (fun _ -> true))

    let private delayWithSome (proc : ProcessUnit option) (time : float) (anode : AsyncNode) =
        let predictor =
            let mutable timer = 0.0
            Delta (fun delta ->
                if proc.IsSome then proc.Value.Invoke delta
                timer <- timer + delta
                timer >= time
            )
        anode |> until predictor
    
    let delayWith (proc : ProcessUnit) (time : float) (anode : AsyncNode) =
        anode |> delayWithSome (Some proc) time
    
    let delay (time : float) (anode : AsyncNode) =
        anode |> delayWithSome None time
    
    let private delayFrameWithSome (proc : ProcessUnit option) (frame : uint) (anode : AsyncNode) =
        let predictor =
            let mutable counter = 0u
            Delta (fun delta ->
                if proc.IsSome then proc.Value.Invoke delta
                counter <- counter + 1u
                counter >= frame
            )
        anode |> until predictor
    
    let delayFrameWith (proc : ProcessUnit) (frame : uint) (anode : AsyncNode) =
        anode |> delayFrameWithSome (Some proc) frame
    
    let delayFrame (frame : uint) (anode : AsyncNode) =
        anode |> delayFrameWithSome None frame
    
    let private waitWithSome<'a> (proc : ProcessUnit option) (waitTask : Task<'a>) (anode : AsyncNode) =
        let log = waitTask |> Task.logWith anode.Ct
        task {
            do! anode |> until (Delta (fun delta ->
                if proc.IsSome then proc.Value.Invoke delta
                log.IsCompleted
            ))
            return log.Result
        }
    
    let waitWith<'a> (proc : ProcessUnit) (waitTask : Task<'a>) (anode : AsyncNode) =
        anode |> waitWithSome (Some proc) waitTask
        
    let wait<'a> (waitTask : Task<'a>) (anode : AsyncNode) =
        anode |> waitWithSome None waitTask
    
    let private waitSignalWithSome<'a> (proc : ProcessUnit option) (obj : GodotObject) (signal : StringName) (anode : AsyncNode) =
        let task = obj |> Signal.awaitWith<'a> anode.Ct signal
        anode |> waitWithSome proc task
    
    let waitSignalWith<'a> (proc : ProcessUnit) (obj : GodotObject) (signal : StringName) (anode : AsyncNode) =
        anode |> waitSignalWithSome<'a> (Some proc) obj signal
    
    let waitSignal<'a> (obj : GodotObject) (signal : StringName) (anode : AsyncNode) =
        anode |> waitSignalWithSome<'a> None obj signal
    
    let private waitTweenWithSome (proc : ProcessUnit option) (tween : Tween) (anode : AsyncNode) =
        tween.Pause ()
        let predictor =
            Delta (fun delta ->
                let result = tween.CustomStep delta
                if result && proc.IsSome then proc.Value.Invoke delta
                result |> not
            )
        task {
            try
                do! anode |> until predictor
            finally
                tween.Kill ()
        }
    
    let waitTweenWith (proc : ProcessUnit) (tween : Tween) (anode : AsyncNode) =
        anode |> waitTweenWithSome (Some proc) tween
    
    let waitTween (tween : Tween) (anode : AsyncNode) =
        anode |> waitTweenWithSome None tween
    
    let private waitEventWithSome (proc : ProcessUnit option) (event: IEvent<'Delegate, 'Args>) (anode : AsyncNode) =
        let task = event |> Event.awaitWith anode.Ct
        anode |> waitWithSome proc task
        
    let waitEventWith (proc : ProcessUnit) (event: IEvent<'Delegate, 'Args>) (anode : AsyncNode) =
        anode |> waitEventWithSome (Some proc) event
        
    let waitEvent (event: IEvent<'Delegate, 'Args>) (anode : AsyncNode) =
        anode |> waitEventWithSome None event
        
    let private runWithSome<'a> (proc : ProcessUnit option) (action : unit -> 'a) (anode : AsyncNode) =
        let t = Task.Run(action, anode.Ct)
        anode |> waitWithSome proc t
        
    let runWith<'a> (proc : ProcessUnit) (action : unit -> 'a) (anode : AsyncNode) =
        anode |> runWithSome (Some proc) action
        
    let run<'a> (action : unit -> 'a) (anode : AsyncNode) =
        anode |> runWithSome None action
    
    let repeat (interval : float) (times : uint) (action : unit -> unit) (anode : AsyncNode) =
        let predictor =
            let mutable counter = 0u
            let mutable timer = 0.0
            Delta (fun delta ->
                timer <- timer + delta
                if timer >= interval then
                    timer <- timer - interval
                    action ()
                    counter <- counter + 1u
                    counter >= times
                else
                    false
            )
        anode |> until predictor
    
type AsyncNode with
    member this.Until (predict : ProcessFunc<bool>) =
        this |> AsyncNode.until predict
    member this.ToProcessThread () =
        this |> AsyncNode.toProcessThread
    member this.Delay (time : float) =
        this |> AsyncNode.delay time
    member this.DelayWith (proc : ProcessUnit) (time : float) =
        this |> AsyncNode.delayWith proc time
    member this.DelayFrame (frame : uint) =
        this |> AsyncNode.delayFrame frame
    member this.DelayFrameWith (proc : ProcessUnit) (frame : uint) =
        this |> AsyncNode.delayFrameWith proc frame
    member this.Wait<'a> (waitTask : Task<'a>) =
        this |> AsyncNode.wait waitTask
    member this.WaitWith<'a> (proc : ProcessUnit) (waitTask : Task<'a>) =
        this |> AsyncNode.waitWith proc waitTask
    member this.WaitSignal<'a> (obj : GodotObject) (signal : StringName) =
        this |> AsyncNode.waitSignal<'a> obj signal
    member this.WaitSignalWith<'a> (proc : ProcessUnit) (obj : GodotObject) (signal : StringName) =
        this |> AsyncNode.waitSignalWith<'a> proc obj signal
    member this.WaitTween (tween : Tween) =
        this |> AsyncNode.waitTween tween
    member this.WaitTweenWith (proc : ProcessUnit) (tween : Tween) =
        this |> AsyncNode.waitTweenWith proc tween
    member this.WaitEvent (event: IEvent<'Delegate, 'Args>) =
        this |> AsyncNode.waitEvent event
    member this.WaitEventWith (proc : ProcessUnit) (event: IEvent<'Delegate, 'Args>) =
        this |> AsyncNode.waitEventWith proc event
    member this.Run<'a> (action : unit -> 'a) =
        this |> AsyncNode.run action
    member this.RunWith<'a> (proc : ProcessUnit) (action : unit -> 'a) =
        this |> AsyncNode.runWith proc action
    member this.Repeat (interval : float) (times : uint) (action : unit -> unit) =
        this |> AsyncNode.repeat interval times action
