namespace Fodot.Core

open System
open System.Collections.Concurrent
open System.Collections.Generic
open FSharp.Concurrent
open Godot

type ProcessFunc<'a> =
    | Unit of (unit -> 'a)
    | Delta of (float -> 'a)
    | Delta32 of (float32 -> 'a)
    
    member this.Invoke (delta : float) =
        match this with
        | Unit f -> f ()
        | Delta f -> f delta
        | Delta32 f -> f (float32 delta)

type ProcessUnit = ProcessFunc<unit>
    
module Engine =
    
    type private ProcessData() =
        member this.Process = ConcurrentDictionary<Guid, ProcessUnit>()
        member this.HasProcess () =
            this.Process.Count > 0
        member this.DoProcess delta =
            this.Process.Values |> Seq.iter (fun f -> f.Invoke delta)

    let private idleMap = WeakMeta<ProcessData>()
    let private physicsMap = WeakMeta<ProcessData>()

    let private cachedIdleUpdate = ConcurrentBag<Node>()
    let private cachedPhysicsUpdate = ConcurrentBag<Node>()
    let private cachedIdleRemove = ConcurrentBag<Node>()
    let private cachedPhysicsRemove = ConcurrentBag<Node>()

    let private updateProcessCache physics node =
        if physics then
            cachedPhysicsUpdate.Add node
        else
            cachedIdleUpdate.Add node
            
    let private updateRemoveCache physics node =
        if physics then
            cachedPhysicsRemove.Add node
        else
            cachedIdleRemove.Add node

    let private getProcessDataMap physics =
        if physics then
            physicsMap
        else
            idleMap

    let private getProcessData physics (node: Node) =
        let map = getProcessDataMap physics
        map |> WeakMeta.getOrAdd node (lazy (
            node.add_TreeEntered (fun () -> node |> updateProcessCache physics)
            node.add_TreeExited (fun () -> node |> updateRemoveCache physics)
            
            ProcessData()
        ))
        
    let hasProcess physics (node: Node) =
        let map = getProcessDataMap physics
        map |> WeakMeta.contains node

    let hasIdleProcess (node: Node) =
        node |> hasProcess false
        
    let hasPhysicsProcess (node: Node) =
        node |> hasProcess true

    let addProcessType (f : ProcessUnit) (physics : bool) (node: Node) =
        let data = node |> getProcessData physics
        if node.IsInsideTree () && data.HasProcess () |> not then
            node |> updateProcessCache physics
        let dict = data.Process
        let id = Guid.NewGuid ()
        dict |> Dict.update id f
        id

    let addProcess (f : unit -> unit) (physics : bool) (node: Node) =
        node |> addProcessType (Unit f) physics
        
    let addDeltaProcess (f : float -> unit) (physics : bool) (node: Node) =
        node |> addProcessType (Delta f) physics

    let addDelta32Process (f : float32 -> unit) (physics : bool) (node: Node) =
        node |> addProcessType (Delta32 f) physics

    let addIdleProcess (f : unit -> unit) (node: Node) =
        node |> addProcess f false
        
    let addPhysicsProcess (f : unit -> unit) (node: Node) =
        node |> addProcess f true
        
    let addIdleDeltaProcess (f : float -> unit) (node: Node) =
        node |> addDeltaProcess f false
       
    let addPhysicsDeltaProcess (f : float -> unit) (node: Node) =
        node |> addDeltaProcess f true
        
    let addIdleDelta32Process (f : float32 -> unit) (node: Node) =
        node |> addDelta32Process f false
        
    let addPhysicsDelta32Process (f : float32 -> unit) (node: Node) =
        node |> addDelta32Process f true

    let private removeProcessWith physics (id: Guid) (node: Node) =
        if node |> hasProcess physics |> not then
            false
        else
            let data = node |> getProcessData physics
            data.Process |> Dict.tryRemove id |> Option.isSome

    let removeIdleProcess (id: Guid) (node: Node) =
        node |> removeProcessWith false id
        
    let removePhysicsProcess (id: Guid) (node: Node) =
        node |> removeProcessWith true id

    let removeProcess (id: Guid) (node: Node) =
        node |> removeIdleProcess id || node |> removePhysicsProcess id

    // node process logic

    let processComparer =
        {
            new IComparer<Node> with
                member this.Compare (x, y) =
                    match x.ProcessPriority - y.ProcessPriority with
                    | 0 -> if x.IsGreaterThan y then 1 else -1
                    | v -> v
        }
        
    let processPhysicsComparer =
        {
            new IComparer<Node> with
                member this.Compare (x, y) =
                    match x.ProcessPhysicsPriority - y.ProcessPhysicsPriority with
                    | 0 -> if x.IsGreaterThan y then 1 else -1
                    | v -> v
        }

    let private cachedProcessNodes = SortedList<Node, ProcessData>(processComparer)
    let private cachedPhysicsProcessNodes = SortedList<Node, ProcessData>(processPhysicsComparer)

    let private treeUpdateProcessCache physics=
        let queue, cache =
            if physics then
                cachedPhysicsUpdate, cachedPhysicsProcessNodes
            else
                cachedIdleUpdate, cachedProcessNodes
        
        for n in queue do
            cache.Add(n, n |> getProcessData physics)
        queue.Clear ()

    let private treeUpdateRemoveCache physics =
        let remove, cache =
            if physics then
                cachedPhysicsRemove, cachedPhysicsProcessNodes
            else
                cachedIdleRemove, cachedProcessNodes
                
        for n in remove do
            cache.Remove n |> ignore
        remove.Clear ()

    let treeUpdateCache () =
        treeUpdateProcessCache true
        treeUpdateProcessCache false
        treeUpdateRemoveCache true
        treeUpdateRemoveCache false

    let private treeDoProcess physics (tree : SceneTree) =
        treeUpdateRemoveCache physics
        treeUpdateProcessCache physics
        let nodes, delta =
            if physics then
                cachedPhysicsProcessNodes,
                tree.GetCurrentScene().GetPhysicsProcessDeltaTime()
            else
                cachedProcessNodes,
                tree.GetCurrentScene().GetProcessDeltaTime()
        nodes |> Seq.iter (fun kv ->
            let n, d = kv.Key, kv.Value
            if GodotObject.IsInstanceValid n && n.CanProcess () then
                d.DoProcess delta
        )

    // entry point

    let private tree =
        lazy (
            // this is optional, it can increase runtime performance
            // otherwise script cache will be built on their first call
            FScript.buildCache ()
            
            let t = Engine.GetMainLoop () :?> SceneTree
            t.add_NodeAdded (fun node -> node |> FScript.init)
            t.add_ProcessFrame (fun () -> t |> treeDoProcess false)
            t.add_PhysicsFrame (fun () -> t |> treeDoProcess true)
            
            t
        )
        
    let getTree () = tree.Value
    
type ProcessConfig =
    {
        Process : ProcessUnit
        Physics : bool
    }
    member this.AddWith (node : Node) =
        node |> Engine.addProcessType this.Process this.Physics
    static member New physics proc=
        {
            Process = proc
            Physics = physics
        }
    static member NewIdle (proc : ProcessUnit) =
        ProcessConfig.New false proc
    static member NewIdle (f : unit -> unit) =
        ProcessConfig.New false (Unit f)
    static member NewIdle (f : float -> unit) =
        ProcessConfig.New false (Delta f)
    static member NewIdle (f : float32 -> unit) =
        ProcessConfig.New false (Delta32 f)
    static member NewPhysics (proc : ProcessUnit) =
        ProcessConfig.New true proc
    static member NewPhysics (f : unit -> unit) =
        ProcessConfig.New true (Unit f)
    static member NewPhysics (f : float -> unit) =
        ProcessConfig.New true (Delta f)
    static member NewPhysics (f : float32 -> unit) =
        ProcessConfig.New true (Delta32 f)