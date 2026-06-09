namespace Moon.Library

open Godot

type PhysicsQueryPointResult =
    {
        Collider : GodotObject
        Rid : Rid
    }
    
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
    
    static member From (result : PhysicsShapeRestInfo3D) =
        {
            Collider = result.GetColliderId() |> GodotObject.InstanceFromId
            Rid = result.GetRid()
            Position = result.GetPoint()
            Normal = result.GetNormal()
            Velocity = result.GetLinearVelocity()
        }
        
type PhysicsQueryMotionCastResult =
    {
        SafeFraction : float32
        UnsafeFraction : float32
    }
    
    static member Default =
        {
            SafeFraction = 1f
            UnsafeFraction = 1f
        }
    
    static member From (result: float32[]) =
        {
            SafeFraction = result[0]
            UnsafeFraction = result[1]
        }
        
type PhysicsQueryMotionTestResult2D =
    {
        Collider : GodotObject
        Rid : Rid
        Velocity : Vector2
        Depth : float32
        Normal : Vector2
        Position : Vector2
        SafeFraction : float32
        UnsafeFraction : float32
        Remainder : Vector2
        Travel : Vector2
    }
    
    static member From (result : PhysicsTestMotionResult2D) =
        {
            Collider = result.GetCollider()
            Rid = result.GetColliderRid()
            Velocity = result.GetColliderVelocity()
            Depth = result.GetCollisionDepth()
            Normal = result.GetCollisionNormal()
            Position = result.GetCollisionPoint()
            SafeFraction = result.GetCollisionSafeFraction()
            UnsafeFraction = result.GetCollisionUnsafeFraction()
            Remainder = result.GetRemainder()
            Travel = result.GetTravel()
        }