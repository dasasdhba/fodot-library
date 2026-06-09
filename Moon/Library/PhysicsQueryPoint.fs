namespace Moon.Library

open Fodot.Module
open Godot

type PhysicsQueryPoint2D(node : CanvasItem, param : PhysicsQueryBasicParameters) =
    
    let state = PhysicsState2D.Get node
    
    new (node : CanvasItem) = PhysicsQueryPoint2D(node, PhysicsQueryBasicParameters.From node)
    
    interface IPhysicsQuery with
        member val Param = param with get, set
    
    member this.QueryGlobal (position : Vector2, ?maxResult : int) =
        state.SpaceState
        |> Option.map (fun s ->
            let maxResult = defaultArg maxResult 32
            
            let query = new PhysicsPointQueryParameters2D()
            query |> (this :> IPhysicsQuery).Param.Attach
            query.Position <- position
            query.CanvasInstanceId <- node.GetInstanceId()
            
            s.IntersectPoint (query, maxResult) |> PhysicsQueryPointResult.From
        )
        |> Option.defaultValue Seq.empty
    
    member this.Query (?offset : Vector2, ?maxResult : int) =
        let position = node |> CanvasItem.getGlobalPosition
        let position = position + (defaultArg offset Vector2.Zero)
        let maxResult = defaultArg maxResult 32
        this.QueryGlobal (position, maxResult)

type PhysicsQueryPoint3D(node : Node3D, param : PhysicsQueryBasicParameters) =
    
    let state = PhysicsState3D.Get node
    
    new (node : Node3D) = PhysicsQueryPoint3D(node, PhysicsQueryBasicParameters.From node)
    
    interface IPhysicsQuery with
        member val Param = param with get, set
    
    member this.QueryGlobal (position : Vector3, ?maxResult : int) =
        state.SpaceState
        |> Option.map (fun s ->
            let maxResult = defaultArg maxResult 32
            
            let query = new PhysicsPointQueryParameters3D()
            query |> (this :> IPhysicsQuery).Param.Attach
            query.Position <- position
            
            s.IntersectPoint (query, maxResult) |> PhysicsQueryPointResult.From
        )
        |> Option.defaultValue Seq.empty
    
    member this.Query (?offset : Vector3, ?maxResult : int) =
        let position = node.GlobalPosition
        let position = position + (defaultArg offset Vector3.Zero)
        let maxResult = defaultArg maxResult 32
        this.QueryGlobal (position, maxResult)
        