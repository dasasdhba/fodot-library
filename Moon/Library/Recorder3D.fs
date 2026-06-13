namespace Moon.Library

open System
open Fodot.Common
open Fodot.Core
open Godot
open Moon.Module

type Recorder3D(node : Node, target : Node3D) =
    
    let mutable velocity = Vector3.Zero
    let mutable motion = Vector3.Zero
    let mutable lastPosition = Vector3.Zero
    let mutable firstRecorded = false
    let mutable disabled = false
    
    let mutable proc = Guid.Empty
    
    do proc <-
        node |> Engine.addPhysicsDelta32Process (fun delta ->
        if GodotObject.IsInstanceValid target |> not then
            Logger.pushWarn $"Recorder3D with {node.GetPath()}: trying to record an invalid target."
            node |> Engine.removePhysicsProcess proc |> ignore
        
        elif disabled then
            velocity <- Vector3.Zero
            motion <- Vector3.Zero
            firstRecorded <- false
            
        else
            
            let pos = target.GlobalPosition
            
            if firstRecorded |> not then
                lastPosition <- pos
                firstRecorded <- true
            else
                motion <- pos - lastPosition
                velocity <- motion / delta
                lastPosition <- pos
    )
    
    new (node : Node3D) = Recorder3D(node, node)
    
    member this.Disabled
        with get () = disabled
        and set v = disabled <- v
    member this.LastVelocity with get() = velocity
    member this.LastMotion with get() = motion
    member this.LastPosition with get() = lastPosition
            
module Recorder3D =
    
    let private map = WeakMeta<Recorder3D>()
    
    let get (n3d : Node3D)=
        n3d |> Node.getSubBinding map (fun n -> Recorder3D(n, n3d))