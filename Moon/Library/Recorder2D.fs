namespace Moon.Library

open Fodot.Common
open Fodot.Core
open Fodot.Stage
open Fodot.Module
open Godot
open Moon.Module

type Recorder2D(node : Node, target : CanvasItem) =
    
    let mutable velocity = Vector2.Zero
    let mutable motion = Vector2.Zero
    let mutable lastPosition = Vector2.Zero
    let mutable firstRecorded = false
    
    new (node : CanvasItem) = Recorder2D(node, node)
    
    member val Disabled = false with get, set
    member val LastVelocity = velocity with get
    member val LastMotion = motion with get
    member val LastPosition = lastPosition with get
    
    member private this.Process = node |> Engine.addPhysicsDelta32Process (fun delta ->
        if GodotObject.IsInstanceValid target |> not then
            Logger.pushWarn $"Recorder2D with {node |> Node.getUniquePath}: trying to record an invalid target."
            node |> Engine.removePhysicsProcess this.Process |> ignore
        
        elif this.Disabled then
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
            
module Recorder2D =
    
    let get (item: CanvasItem)=
        item |> Node.getSubBinding (fun n -> Recorder2D(n, item))