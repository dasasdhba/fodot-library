namespace Moon.Library

open Godot

type IPhysicsQueryResult =
    abstract member Collider : GodotObject with get
    abstract member Rid : Rid with get

type PhysicsQueryResult =
    {
        Collider : GodotObject
        Rid : Rid
    }
    
    interface IPhysicsQueryResult with
        member this.Collider = this.Collider
        member this.Rid = this.Rid
    
    static member From (result : PhysicsShapeQueryResults2D) =
        seq {
            for i in 0 .. result.GetCollisionCount() - 1 do
                let col = result.GetCollider i
                let rid = result.GetRid i
                yield { Collider = col; Rid = rid }
        }
        
    static member From (result : PhysicsShapeQueryResults3D) =
        seq {
            for i in 0 .. result.GetCollisionCount() - 1 do
                let col = result.GetCollider i
                let rid = result.GetRid i
                yield { Collider = col; Rid = rid }
        }

type PhysicsQueryRayResult2D =
    {
        Collider : GodotObject
        Rid : Rid
        Position : Vector2
        Normal : Vector2
    }
    
    interface IPhysicsQueryResult with
        member this.Collider = this.Collider
        member this.Rid = this.Rid
    
    static member From (result : PhysicsRayQueryResult2D) =
        {
            Collider = result.GetCollider()
            Rid = result.GetRid()
            Position = result.GetPosition()
            Normal = result.GetNormal()
        }

type PhysicsQueryRayResult3D =
    {
        Collider : GodotObject
        Rid : Rid
        Position : Vector3
        Normal : Vector3
    }
    
    interface IPhysicsQueryResult with
        member this.Collider = this.Collider
        member this.Rid = this.Rid
    
    static member From (result : PhysicsRayQueryResult3D) =
        {
            Collider = result.GetCollider()
            Rid = result.GetRid()
            Position = result.GetPosition()
            Normal = result.GetNormal()
        }

type PhysicsQueryShapeResult2D =
    {
        Collider : GodotObject
        Rid : Rid
        Position : Vector2
        Normal : Vector2
        Velocity : Vector2
    }
    
    interface IPhysicsQueryResult with
        member this.Collider = this.Collider
        member this.Rid = this.Rid
    
    static member From (result : PhysicsShapeRestInfo2D) =
        {
            Collider = result.GetColliderId() |> GodotObject.InstanceFromId
            Rid = result.GetRid()
            Position = result.GetPoint()
            Normal = result.GetNormal()
            Velocity = result.GetLinearVelocity()
        }
        
type PhysicsQueryShapeResult3D =
    {
        Collider : GodotObject
        Rid : Rid
        Position : Vector3
        Normal : Vector3
        Velocity : Vector3
    }
    
    interface IPhysicsQueryResult with
        member this.Collider = this.Collider
        member this.Rid = this.Rid
    
    static member From (result : PhysicsShapeRestInfo3D) =
        {
            Collider = result.GetColliderId() |> GodotObject.InstanceFromId
            Rid = result.GetRid()
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
    
    static member From (result: float32[]) =
        {
            SafeFraction = result[0]
            UnsafeFraction = result[1]
        }

module PhysicsQueryResult =
    
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