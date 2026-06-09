namespace Moon.Library

open Fodot.Module
open Godot

type PhysicsQueryRaycast2D(node : CanvasItem, param : PhysicsQueryBasicParameters) =
    
    let state = PhysicsState2D.Get node
    
    new (node : CanvasItem) = PhysicsQueryRaycast2D(node, PhysicsQueryBasicParameters.From node)
    
    interface IPhysicsQuery with
        member val Param = param with get, set
    
    member val HitFromInside = false with get, set
    
    member this.QueryGlobal (from : Vector2, to' : Vector2, ?maxResult : int) =
        let maxResult = defaultArg maxResult 32
        
        use query = new PhysicsRayQueryParameters2D()
        query |> (this :> IPhysicsQuery).Param.Attach
        query.From <- from
        query.To <- to'
        query.HitFromInside <- this.HitFromInside
        
        query
        |> Seq.unfold (fun q ->
            state.SpaceState
            |> Option.bind (fun s -> s.IntersectRay q |> Option.ofObj)
            |> Option.map (fun r ->
                let res = PhysicsQueryRayResult2D.From r
                q.Exclude <-
                    let ex = q.Exclude
                    ex.Add res.Rid
                    ex
                res, q
            )
        )
        |> Seq.truncate maxResult
    
    member this.Query (target : Vector2, ?offset : Vector2, ?maxResult : int) =
        let from = node |> CanvasItem.getGlobalPosition
        let from = from + (defaultArg offset Vector2.Zero)
        let to' = from + target
        let maxResult = defaultArg maxResult 32
        this.QueryGlobal (from, to', maxResult)

type PhysicsQueryRaycast3D(node : Node3D, param : PhysicsQueryBasicParameters) =
    
    let state = PhysicsState3D.Get node
    
    new (node : Node3D) = PhysicsQueryRaycast3D(node, PhysicsQueryBasicParameters.From node)
    
    interface IPhysicsQuery with
        member val Param = param with get, set
    
    member val HitFromInside = false with get, set
    member val HitBackFaces = true with get, set
    
    member this.QueryGlobal (from : Vector3, to' : Vector3, ?maxResult : int) =
        let maxResult = defaultArg maxResult 32
        
        use query = new PhysicsRayQueryParameters3D()
        query |> (this :> IPhysicsQuery).Param.Attach
        query.From <- from
        query.To <- to'
        query.HitFromInside <- this.HitFromInside
        query.HitBackFaces <- this.HitBackFaces
        
        query
        |> Seq.unfold (fun q ->
            state.SpaceState
            |> Option.bind (fun s -> s.IntersectRay q |> Option.ofObj)
            |> Option.map (fun r ->
                let res = PhysicsQueryRayResult3D.From r
                q.Exclude <-
                    let ex = q.Exclude
                    ex.Add res.Rid
                    ex
                res, q
            )
        )
        |> Seq.truncate maxResult
    
    member this.Query (target : Vector3, ?offset : Vector3, ?maxResult : int) =
        let from = node.GlobalPosition
        let from = from + (defaultArg offset Vector3.Zero)
        let to' = from + target
        let maxResult = defaultArg maxResult 32
        this.QueryGlobal (from, to', maxResult)