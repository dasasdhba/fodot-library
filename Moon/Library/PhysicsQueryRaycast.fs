namespace Moon.Library

open Fodot.Module
open Godot

type PhysicsQueryRaycast2D(node : CanvasItem, param : PhysicsQueryBasicParameters) =
    
    let state = PhysicsState2D.Get node
    
    new (node : CanvasItem) = PhysicsQueryRaycast2D(node, PhysicsQueryBasicParameters.From node)
    
    interface IPhysicsQuery with
        member val Param = param with get, set
    
    member val HitFromInside = false with get, set
    member val MaxResult = 32 with get, set
    
    member this.QueryGlobal (from : Vector2, to' : Vector2, ?maxResult : int, ?hitFromInside : bool) =
        state.SpaceState
        |> Option.map (fun dss ->
            let maxResult = defaultArg maxResult this.MaxResult
            let hitFromInside = defaultArg hitFromInside this.HitFromInside
        
            let query = new PhysicsRayQueryParameters2D()
            query |> (this :> IPhysicsQuery).Param.Attach
            query.From <- from
            query.To <- to'
            query.HitFromInside <- hitFromInside
            
            () |> Seq.unfold (fun _ ->
                dss.IntersectRay query
                |> Option.ofObj
                |> Option.map (fun r ->
                    let res = PhysicsQueryRayResult2D.From r
                    query.Exclude <-
                        let ex = query.Exclude
                        ex.Add res.Rid
                        ex
                    res, ()
                )
            )
            |> Seq.truncate maxResult
        )
        |> Option.defaultValue Seq.empty
    
    member this.Query (target : Vector2, ?offset : Vector2, ?maxResult : int, ?hitFromInside : bool) =
        let from = node |> CanvasItem.getGlobalPosition
        let from = from + (defaultArg offset Vector2.Zero)
        let to' = from + target
        this.QueryGlobal (from, to', ?maxResult = maxResult, ?hitFromInside = hitFromInside)

type PhysicsQueryRaycast3D(node : Node3D, param : PhysicsQueryBasicParameters) =
    
    let state = PhysicsState3D.Get node
    
    new (node : Node3D) = PhysicsQueryRaycast3D(node, PhysicsQueryBasicParameters.From node)
    
    interface IPhysicsQuery with
        member val Param = param with get, set
    
    member val HitFromInside = false with get, set
    member val HitBackFaces = true with get, set
    member val MaxResult = 32 with get, set
    
    member this.QueryGlobal (from : Vector3, to' : Vector3, ?maxResult : int, ?hitFromInside : bool, ?hitBackFaces : bool) =
        state.SpaceState
        |> Option.map (fun dss ->
            let maxResult = defaultArg maxResult this.MaxResult
            let hitFromInside = defaultArg hitFromInside this.HitFromInside
            let hitBackFaces = defaultArg hitBackFaces this.HitBackFaces
        
            let query = new PhysicsRayQueryParameters3D()
            query |> (this :> IPhysicsQuery).Param.Attach
            query.From <- from
            query.To <- to'
            query.HitFromInside <- hitFromInside
            query.HitBackFaces <- hitBackFaces
            
            () |> Seq.unfold (fun _ ->
                dss.IntersectRay query
                |> Option.ofObj
                |> Option.map (fun r ->
                    let res = PhysicsQueryRayResult3D.From r
                    query.Exclude <-
                        let ex = query.Exclude
                        ex.Add res.Rid
                        ex
                    res, ()
                )
            )
            |> Seq.truncate maxResult
        )
        |> Option.defaultValue Seq.empty
    
    member this.Query (target : Vector3, ?offset : Vector3, ?maxResult : int, ?hitFromInside : bool, ?hitBackFaces : bool) =
        let from = node.GlobalPosition
        let from = from + (defaultArg offset Vector3.Zero)
        let to' = from + target
        this.QueryGlobal (from, to', ?maxResult = maxResult, ?hitFromInside = hitFromInside, ?hitBackFaces = hitBackFaces)