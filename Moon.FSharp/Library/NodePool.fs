namespace Moon.Library

open System
open System.Collections.Concurrent
open Godot
open Fodot.Common
open Fodot.Core

type NodePool (scene : PackedScene) =
    inherit RefCounted ()
    
    let pool = ConcurrentQueue<Node>()
    let mutable disposed = false
        
    member val Disposed = disposed with get
        
    member this.Store count =
        for i in 0 .. count - 1 do
            let node = scene |> PackedScene.instantiate
            node |> GodotObject.setMeta "_moon_node_pool" this
            node.add_TreeExited (fun _ ->
                if not disposed then
                    this.Return node
            )
            pool.Enqueue(node)
        
    member this.Return(node) =
        pool.Enqueue(node)
        
    member this.Get() =
        if disposed then
            failwith $"NodePool with {scene.ResourcePath} has been disposed."
        
        match pool.TryDequeue() with
        
        | true, node -> node
        | false, _ ->
        #if TOOLS
            Logger.pushWarn $"NodePool with {scene.ResourcePath} is running out! Consider increasing the pool size."
        #endif
            scene |> PackedScene.instantiate
        
    member this.GetAs<'a when 'a :> Node>() =
        this.Get() :?> 'a
    
    interface IDisposable with
        member this.Dispose() =
            disposed <- true
            pool |> Seq.iter _.QueueFree()
            pool.Clear ()
            
module NodePool =
    
    let createPool (scene : PackedScene) (maxCount : int) =
        let pool = new NodePool(scene)
        pool.Store maxCount
        pool
        
    let create (scene : PackedScene) (maxCount : int) (node : Node) =
        let pool = createPool scene maxCount
        node |> Node.bindDisposable pool
        pool
        
    let returnPool (node : Node) =
        let canReturn =
            node
            |> GodotObject.tryGetMetaAs<NodePool> "_moon_node_pool"
            |> Option.map (fun p -> p.Disposed |> not)
            |> Option.defaultValue false
            
        if canReturn then
            node |> Node.getParent |> Node.removeChild node
        else
            node.QueueFree()
            
        canReturn