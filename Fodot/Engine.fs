namespace Fodot

open System
open FSharp.Generic
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
    
[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Engine =
    
    type private ProcessData() =
        member val Process : (Guid * ProcessUnit) list = [] with get, set
        member this.HasProcess () =
            this.Process |> List.isEmpty |> not
        member this.DoProcess delta =
            this.Process |> List.iter (fun (_, f) ->
                try
                    f.Invoke delta
                with
                | ex -> Logger.pushError ex
            )

    let private idleMap = WeakMeta<ProcessData>()
    let private physicsMap = WeakMeta<ProcessData>()
    
    let private idleFlush =
        SortedFlushIdlePool<Node * ProcessData>(fst)

    let private physicsFlush =
        SortedFlushPhysicsPool<Node * ProcessData>(fst)

    let private trackFlush physics (node: Node, data : ProcessData) =
        if physics then
            physicsFlush.Track(node, data)
        else
            idleFlush.Track(node, data)

    let private getProcessDataMap physics =
        if physics then
            physicsMap
        else
            idleMap

    let private getProcessData physics (node: Node) =
        let map = getProcessDataMap physics
        map |> WeakMeta.getOrAdd node (fun () ->
            let data = ProcessData()
            (node, data) |> trackFlush physics
            data
        )
        
    let hasProcess physics (node: Node) =
        let map = getProcessDataMap physics
        map |> WeakMeta.contains node

    let hasIdleProcess (node: Node) =
        node |> hasProcess false
        
    let hasPhysicsProcess (node: Node) =
        node |> hasProcess true

    let addProcessType (physics : bool) (f : ProcessUnit) (node: Node) =
        let data = node |> getProcessData physics
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
            |> List.remove (fun (i, _) -> id = i)
            |> Option.map (fun r -> data.Process <- r)
            |> Option.isSome

    let removeIdleProcess (id: Guid) (node: Node) =
        node |> removeProcessWith false id
        
    let removePhysicsProcess (id: Guid) (node: Node) =
        node |> removeProcessWith true id

    let removeProcess (id: Guid) (node: Node) =
        node |> removeIdleProcess id || node |> removePhysicsProcess id

    // node process logic

    let private treeDoProcess physics (tree : SceneTree) =
        let (nodes : SortedFlushPool<Node * ProcessData>), delta =
            if physics then
                physicsFlush,
                tree.GetCurrentScene().GetPhysicsProcessDeltaTime()
            else
                idleFlush,
                tree.GetCurrentScene().GetProcessDeltaTime()
        
        nodes.Flush ()
        nodes.Iter ()
        |> Seq.filter (fst >> _.CanProcess())
        |> Seq.iter (snd >> _.DoProcess(delta))

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
    
    let callGroup (group : StringName) (method : StringName) (args : 'a) =
        getTree().CallGroup(group, method, args |> Variant.fromTuple)
        
    let invokeGroup (group : StringName) (method : StringName)=
        getTree().CallGroup(group, method)
    
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
        
// gds process binding

[<FScript("fodot_process")>]
type private GdProcess(node : Node) =
    static let method = new StringName "_fs_process"
    do
        node |> Engine.addIdleDeltaProcess (fun delta ->
            node.Call(method, delta) |> ignore
        ) |> ignore
        
    static member Method = method
        
[<FScript("fodot_physics_process")>]
type private GdPhysicsProcess(node : Node) =
    static let method = new StringName "_fs_physics_process"
    do
        node |> Engine.addPhysicsDeltaProcess (fun delta ->
            node.Call(method, delta) |> ignore
        ) |> ignore
        
    static member Method = method
    
[<FScript("fodot_hack_process")>]
type private GdHackProcess(node : Node) =
    do node.add_Ready (fun _ ->
        node
        |> GodotObject.tryInvokeAs<StringName> GdProcess.Method
        |> Option.iter (fun c ->
            node |> Engine.addIdleDeltaProcess (fun delta ->
                node.Call(c, delta) |> ignore
            ) |> ignore
        )
    )
    
[<FScript("fodot_hack_physics_process")>]
type private GdHackPhysicsProcess(node : Node) =
    do node.add_Ready (fun _ ->
        node
        |> GodotObject.tryInvokeAs<StringName> GdPhysicsProcess.Method
        |> Option.iter (fun c ->
            node |> Engine.addPhysicsDeltaProcess (fun delta ->
               node.Call(c, delta) |> ignore
            ) |> ignore
        )
    )
