namespace Moon.Library

open Godot
open Moon.Interface

type IPhysicsQueryResult =
    abstract member Collider : GodotObject with get
    abstract member Rid : Rid with get
    abstract member Shape : int with get

type PhysicsQueryResult =
    {
        Collider : GodotObject
        Rid : Rid
        Shape : int
    }
    
    interface IPhysicsQueryResult with
        member this.Collider = this.Collider
        member this.Rid = this.Rid
        member this.Shape = this.Shape
    
    static member From (result : PhysicsShapeQueryResults2D) =
        seq {
            for i in 0 .. result.GetCollisionCount() - 1 do
                let col = result.GetCollider i
                let rid = result.GetRid i
                let shape = result.GetShape i
                yield {
                    Collider = col
                    Rid = rid
                    Shape = shape
                }
        }
        
    static member From (result : PhysicsShapeQueryResults3D) =
        seq {
            for i in 0 .. result.GetCollisionCount() - 1 do
                let col = result.GetCollider i
                let rid = result.GetRid i
                let shape = result.GetShape i
                yield {
                    Collider = col
                    Rid = rid
                    Shape = shape
                }
        }

type PhysicsQueryRayResult2D =
    {
        Collider : GodotObject
        Rid : Rid
        Shape : int
        Position : Vector2
        Normal : Vector2
    }
    
    interface IPhysicsQueryResult with
        member this.Collider = this.Collider
        member this.Rid = this.Rid
        member this.Shape = this.Shape
    
    static member From (result : PhysicsRayQueryResult2D) =
        {
            Collider = result.GetCollider()
            Rid = result.GetRid()
            Shape = result.GetShape()
            Position = result.GetPosition()
            Normal = result.GetNormal()
        }

type PhysicsQueryRayResult3D =
    {
        Collider : GodotObject
        Rid : Rid
        Shape : int
        Position : Vector3
        Normal : Vector3
    }
    
    interface IPhysicsQueryResult with
        member this.Collider = this.Collider
        member this.Rid = this.Rid
        member this.Shape = this.Shape
    
    static member From (result : PhysicsRayQueryResult3D) =
        {
            Collider = result.GetCollider()
            Rid = result.GetRid()
            Shape = result.GetShape()
            Position = result.GetPosition()
            Normal = result.GetNormal()
        }

type PhysicsQueryShapeResult2D =
    {
        Collider : GodotObject
        Rid : Rid
        Shape : int
        Position : Vector2
        Normal : Vector2
        Velocity : Vector2
    }
    
    interface IPhysicsQueryResult with
        member this.Collider = this.Collider
        member this.Rid = this.Rid
        member this.Shape = this.Shape
    
    static member From (result : PhysicsShapeRestInfo2D) =
        {
            Collider = result.GetColliderId() |> GodotObject.InstanceFromId
            Rid = result.GetRid()
            Shape = result.GetShape()
            Position = result.GetPoint()
            Normal = result.GetNormal()
            Velocity = result.GetLinearVelocity()
        }
        
type PhysicsQueryShapeResult3D =
    {
        Collider : GodotObject
        Rid : Rid
        Shape : int
        Position : Vector3
        Normal : Vector3
        Velocity : Vector3
    }
    
    interface IPhysicsQueryResult with
        member this.Collider = this.Collider
        member this.Rid = this.Rid
        member this.Shape = this.Shape
    
    static member From (result : PhysicsShapeRestInfo3D) =
        {
            Collider = result.GetColliderId() |> GodotObject.InstanceFromId
            Rid = result.GetRid()
            Shape = result.GetShape()
            Position = result.GetPoint()
            Normal = result.GetNormal()
            Velocity = result.GetLinearVelocity()
        }
        
type PhysicsQueryMotionResult =
    {
        SafeFraction : float32
        UnsafeFraction : float32
    }
    
    static member Default =
        {
            SafeFraction = 1f
            UnsafeFraction = 1f
        }
    
    static member Zero =
        {
            SafeFraction = 0f
            UnsafeFraction = 0f
        }
    
    static member FromValue(safe, ?unsafe) =
        {
            SafeFraction = safe
            UnsafeFraction = defaultArg unsafe safe
        }
    
    static member From (result: float32[]) =
        {
            SafeFraction = result[0]
            UnsafeFraction = result[1]
        }

module PhysicsQueryResult =

    let getOneWayParameters2D (r : IPhysicsQueryResult) =
        match r.Collider with
        | :? CanvasItem as c ->
            if PhysicsServer2D.BodyIsShapeSetAsOneWayCollision(r.Rid, r.Shape) &&
               PhysicsServer2D.ShapeIsOneWayCollisionAllowed(
                    PhysicsServer2D.BodyGetShape(r.Rid, r.Shape)
               ) then
                let margin = PhysicsServer2D.BodyGetShapeOneWayCollisionMargin(r.Rid, r.Shape)
                let dir = PhysicsServer2D.BodyGetShapeOneWayCollisionDirection(r.Rid, r.Shape)
                let dir =
                    dir
                    |> c.GetGlobalTransform().BasisXform
                    |> _.Normalized()
                Some (dir, margin)
            else
                None
        | _ -> None
    
    let getOneWayDirection2D (r : IPhysicsQueryResult) =
        r
        |> getOneWayParameters2D
        |> Option.map fst
    
    let getOneWayParameters3D (r : IPhysicsQueryResult) =
        match r.Collider with
        | :? CollisionObject3D as col ->
            let owner =
                r.Shape
                |> col.ShapeFindOwner
                |> col.ShapeOwnerGetOwner
            match owner :> obj with
            | :? IPlatformShape3D as p when p.OneWayCollision ->
                let margin = p.OneWayCollisionMargin
                let dir = p.OneWayCollisionDirection.Normalized ()
                let dir =
                    col.GetGlobalTransform().Basis * dir
                    |> _.Normalized()
                Some (dir, margin)
            | _ ->
                None
        | _ -> None
    
    let getOneWayDirection3D (r : IPhysicsQueryResult) =
        r
        |> getOneWayParameters3D
        |> Option.map fst
        
    let chooseAndExclude<'a, 'b when 'a :> IPhysicsQueryResult>
        (query : IPhysicsQuery)
        (pattern : 'a -> 'b option)
        (results : 'a seq) : 'b seq =
        
        results
        |> Seq.choose (fun r ->
            match pattern r with
            | Some s -> Some s
            | None ->
                query |> PhysicsQuery.addExclude r.Rid
                None
        )