namespace Fodot.Core

open System
open System.Collections.Concurrent
open System.Collections.Generic
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
        member val Process : (Guid * ProcessUnit) list = [] with get, set
        member this.HasProcess () =
            this.Process |> List.isEmpty |> not
        member this.DoProcess delta =
            this.Process |> List.iter (fun (_, f) ->
                f.Invoke delta
            )

    let private idleMap = WeakMeta<ProcessData>()
    let private physicsMap = WeakMeta<ProcessData>()

    let private cachedIdleUpdate = ConcurrentQueue<Node>()
    let private cachedPhysicsUpdate = ConcurrentQueue<Node>()
    let private cachedIdleRemove = ConcurrentQueue<Node>()
    let private cachedPhysicsRemove = ConcurrentQueue<Node>()

    let private updateProcessCache physics node =
        if physics then
            cachedPhysicsUpdate.Enqueue node
        else
            cachedIdleUpdate.Enqueue node
            
    let private updateRemoveCache physics node =
        if physics then
            cachedPhysicsRemove.Enqueue node
        else
            cachedIdleRemove.Enqueue node

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

    let addProcessType (physics : bool) (f : ProcessUnit) (node: Node) =
        let data = node |> getProcessData physics
        if node.IsInsideTree () && data.HasProcess () |> not then
            node |> updateProcessCache physics
        let id = Guid.NewGuid ()
        data.Process <- data.Process @ [id, f]
        id

    let addProcess (physics : bool) (f : unit -> unit) (node: Node) =
        node |> addProcessType physics (Unit f)
        
    let addDeltaProcess (physics : bool) (f : float -> unit) (node: Node) =
        node |> addProcessType physics (Delta f)

    let addDelta32Process (physics : bool) (f : float32 -> unit) (node: Node) =
        node |> addProcessType physics (Delta32 f)

    let addIdleProcess (f : unit -> unit) (node: Node) =
        node |> addProcess false f
        
    let addPhysicsProcess (f : unit -> unit) (node: Node) =
        node |> addProcess true f
        
    let addIdleDeltaProcess (f : float -> unit) (node: Node) =
        node |> addDeltaProcess false f
       
    let addPhysicsDeltaProcess (f : float -> unit) (node: Node) =
        node |> addDeltaProcess true f
        
    let addIdleDelta32Process (f : float32 -> unit) (node: Node) =
        node |> addDelta32Process false f
        
    let addPhysicsDelta32Process (f : float32 -> unit) (node: Node) =
        node |> addDelta32Process true f

    let private removeProcessWith physics (id: Guid) (node: Node) =
        if node |> hasProcess physics |> not then
            false
        else
            let data = node |> getProcessData physics
            data.Process
            |> List.tryFindIndex (fun (i, _) -> id = i)
            |> Option.map (fun i ->
                data.Process <- data.Process |> List.removeAt i)
            |> Option.isSome

    let removeIdleProcess (id: Guid) (node: Node) =
        node |> removeProcessWith false id
        
    let removePhysicsProcess (id: Guid) (node: Node) =
        node |> removeProcessWith true id

    let removeProcess (id: Guid) (node: Node) =
        node |> removeIdleProcess id || node |> removePhysicsProcess id

    // node process logic
    
    let private nodeComparer physics (x: Node) (y: Node) =
        let vx = GodotObject.IsInstanceValid x |> Convert.ToInt32
        let vy = GodotObject.IsInstanceValid y |> Convert.ToInt32
        if vx + vy < 2 then
            vx - vy
        else
            let result =
                if physics then
                    x.ProcessPhysicsPriority - y.ProcessPhysicsPriority
                else
                    x.ProcessPriority - y.ProcessPriority
            match result with
            | 0 -> if x.IsGreaterThan y then 1 else -1
            | v -> v

    let processComparer =
        {
            new IComparer<Node> with
                member this.Compare (x, y) =
                    nodeComparer false x y
        }
        
    let processPhysicsComparer =
        {
            new IComparer<Node> with
                member this.Compare (x, y) =
                    nodeComparer true x y
        }

    let private cachedProcessNodes = SortedList<Node, ProcessData>(processComparer)
    let private cachedPhysicsProcessNodes = SortedList<Node, ProcessData>(processPhysicsComparer)

    let private treeUpdateProcessCache physics=
        let queue, cache =
            if physics then
                cachedPhysicsUpdate, cachedPhysicsProcessNodes
            else
                cachedIdleUpdate, cachedProcessNodes
        
        let mutable n = null
        while queue.TryDequeue(&n) do
            cache.Add(n, n |> getProcessData physics)

    let private treeUpdateRemoveCache physics =
        let remove, cache =
            if physics then
                cachedPhysicsRemove, cachedPhysicsProcessNodes
            else
                cachedIdleRemove, cachedProcessNodes
                
        let mutable n = null
        while remove.TryDequeue(&n) do
            cache.Remove n |> ignore

    let treeUpdateCache () =
        treeUpdateRemoveCache true
        treeUpdateRemoveCache false
        treeUpdateProcessCache true
        treeUpdateProcessCache false

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
        node |> Engine.addProcessType this.Physics this.Process
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