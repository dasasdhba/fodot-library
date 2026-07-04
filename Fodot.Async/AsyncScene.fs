namespace Fodot.Async

open FSharp.Concurrent
open Fodot
open System
open System.Collections.Concurrent
open System.Threading.Tasks
open Godot

type AsyncScenePool() =
    let queueLock = obj()
    let mutable queuedAdd : PackedScene list = []
    let mutable queuedRemove : PackedScene list = []
    let pool =
        ConcurrentDictionary<PackedScene, ConcurrentQueue<Node>> ()
    let mutable addTask : Task option = None
    
    member this.Update (scene : PackedScene) =
        let node = PackedScene.instantiate scene
        
        pool.AddOrUpdate(
            scene, (fun s ->
                let r = ConcurrentQueue<Node>()
                r.Enqueue(node)
                r
            ), (fun s q ->
                q.Enqueue(node)
                q
            )
        ) |> ignore
    
    member this.UpdateMultiple (count : int) (scene : PackedScene) =
        for _ in 1..count do
            this.Update scene
    
    member this.Get (scene : PackedScene) =
        let queue = pool.GetOrAdd(scene, fun s ->
            ConcurrentQueue<Node>()
        )
        
        queue
        |> Queue.tryDequeue
        |> Option.map Ok
        |> Option.defaultWith (fun _ ->
            Logger.pushWarn $"{scene} at {scene.ResourcePath} has not been cached yet, try to increase initial count, or use pooling instead."
            PackedScene.instantiate scene |> Result.Error
        )
    
    member private this.RemoveWith count (scene : PackedScene) =
        let matching, remain =
            queuedAdd |> List.partition (fun s -> s = scene)
        
        if matching.Length >= count then
            queuedAdd <- matching[count..] @ remain
        else
            queuedAdd <- remain
            
            let remain = count - matching.Length
            let queue = pool.GetOrAdd(scene, fun s ->
                ConcurrentQueue<Node>()
            )
            
            let rec free c =
                if c <= 0 then () else
                
                match queue |> Queue.tryDequeue with
                | Some n ->
                    GDThread.post n.QueueFree
                    free (c - 1)
                | _ -> ()
            
            free remain
            
    member private this.Remove () =
        queuedRemove
        |> List.countBy id
        |> List.iter (fun (s, i) -> this.RemoveWith i s)
        
        queuedRemove <- []
    
    member private this.CreateAddTask () =
        Task.Run(fun () ->
            while queuedAdd.Length > 0 do
                lock queueLock (fun () ->
                    let scene = queuedAdd.Head
                    this.Update scene
                    queuedAdd <- queuedAdd.Tail
                ) 
        )
    
    member private this.CreateRemoveTask () =
        Task.Run(fun () ->
            lock queueLock (fun () ->
                this.Remove ()
            )
        )
    
    member this.AddList (scene : PackedScene list) =
        lock queueLock (fun () ->
            queuedAdd <- queuedAdd @ scene
        )
        
        if addTask.IsNone || addTask.Value.IsCompleted then
            addTask <- Some (this.CreateAddTask ())
    
    member this.AddMultiple (count : int) (scene : PackedScene) =
        this.AddList [for _ in 1..count -> scene]
    
    member this.Add (scene : PackedScene) =
        this.AddMultiple 1 scene
    
    member this.RemoveList (scene : PackedScene list) =
        lock queueLock (fun () ->
            queuedRemove <- queuedRemove @ scene
        )
        
        this.CreateRemoveTask () |> ignore
        
    member this.RemoveMultiple (count : int) (scene : PackedScene) =
        this.RemoveList [for _ in 1..count -> scene]
    
    member this.Remove (scene : PackedScene) =
        this.RemoveMultiple 1 scene
    
type AsyncSceneConfig =
    {
        Pool : AsyncScenePool
        Scene : PackedScene
        MaxCount : int
        InitialCount : int
    }

type AsyncScene<'a when 'a :> Node> (cfg : AsyncSceneConfig) =
    let mutable disposed = false
    
    do
        let initCount = min cfg.MaxCount cfg.InitialCount
        if initCount > 0 then
            cfg.Pool.UpdateMultiple initCount cfg.Scene
        
        let remain = cfg.MaxCount - initCount
        if remain > 0 then
            cfg.Pool.AddMultiple remain cfg.Scene
        
    member this.Get () =
        if disposed then
            failwith $"AsyncScene at {cfg.Scene.ResourcePath} has been disposed."
        
        match cfg.Pool.Get cfg.Scene with
        | Ok node ->
            cfg.Pool.Add cfg.Scene
            node :?> 'a
        | Result.Error node ->
            node :?> 'a
        
    interface IDisposable with
        member this.Dispose () =
            disposed <- true
            cfg.Pool.RemoveMultiple cfg.MaxCount cfg.Scene
        
module AsyncScene =
    
    let globalPool = AsyncScenePool ()
    
    let createCfg (scene : PackedScene) (maxCount : int) (initialCount : int) =
        {
            Pool = globalPool
            Scene = scene
            MaxCount = maxCount
            InitialCount = initialCount
        }
    
    let fromCfg<'a when 'a :> Node> (cfg : AsyncSceneConfig) =
        new AsyncScene<'a>(cfg)
        
    let fromCfgWith<'a when 'a :> Node> (node : Node) (cfg : AsyncSceneConfig) =
        let scene = fromCfg<'a> cfg
        node |> Node.bindDisposable scene
        scene
        
    let create<'a when 'a :> Node> (scene : PackedScene) (maxCount : int) (initialCount : int) (node : Node)=
        let cfg = createCfg scene maxCount initialCount
        fromCfgWith<'a> node cfg
        
type AsyncSceneConfig with
    member this.CreateWith<'a when 'a :> Node> node =
        this |> AsyncScene.fromCfgWith<'a> node
    static member New (scene : PackedScene) (maxCount : int) (initialCount : int) =
        AsyncScene.createCfg scene maxCount initialCount
