namespace Moon.Library

open Fodot.Common
open Fodot.Core
open Fodot.Stage
open Godot
open Moon.Module

type Recorder3D(node : Node, target : Node3D) =
    
    let mutable velocity = Vector3.Zero
    let mutable motion = Vector3.Zero
    let mutable lastPosition = Vector3.Zero
    let mutable firstRecorded = false
    
    new (node : Node3D) = Recorder3D(node, node)
    
    member val Disabled = false with get, set
    member val LastVelocity = velocity with get
    member val LastMotion = motion with get
    member val LastPosition = lastPosition with get
    
    member private this.Process = node |> Engine.addPhysicsDelta32Process (fun delta ->
        if GodotObject.IsInstanceValid target |> not then
            Logger.pushWarn $"Recorder3D with {node |> Node.getUniquePath}: trying to record an invalid target."
            node |> Engine.removePhysicsProcess this.Process |> ignore
        
        elif this.Disabled then
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
            
module Recorder3D =
    
    let get (n3d : Node3D)=
        n3d |> Node.getSubBinding (fun n -> Recorder3D(n, n3d))