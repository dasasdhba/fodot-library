namespace Moon.Library

open System
open Fodot.Common
open Fodot.Core
open Fodot.Module
open Godot
open Moon.Module

type Recorder2D(node : Node, target : CanvasItem) =
    
    let mutable velocity = Vector2.Zero
    let mutable motion = Vector2.Zero
    let mutable lastPosition = Vector2.Zero
    let mutable firstRecorded = false
    let mutable disabled = false
    
    let mutable proc = Guid.Empty
    
    do proc <-
        node |> Engine.addPhysicsDelta32Process (fun delta ->
        if GodotObject.IsInstanceValid target |> not then
            Logger.pushWarn $"Recorder2D with {node.GetPath()}: trying to record an invalid target."
            node |> Engine.removePhysicsProcess proc |> ignore
        
        elif disabled then
            velocity <- Vector2.Zero
            motion <- Vector2.Zero
            firstRecorded <- false
            
        else
            
            let pos = target |> CanvasItem.getGlobalPosition
            
            if firstRecorded |> not then
                lastPosition <- pos
                firstRecorded <- true
            else
                motion <- pos - lastPosition
                velocity <- motion / delta
                lastPosition <- pos
    )
    
    new (node : CanvasItem) = Recorder2D(node, node)
    
    member this.Disabled
        with get () = disabled
        and set v = disabled <- v
    member this.LastVelocity with get() = velocity
    member this.LastMotion with get() = motion
    member this.LastPosition with get() = lastPosition
            
module Recorder2D =
    
    let private map = WeakMeta<Recorder2D>()
    
    let get (item: CanvasItem)=
        item |> Node.getSubBindingFront map (fun n -> Recorder2D(n, item))