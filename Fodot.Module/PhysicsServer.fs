module Fodot.Module.PhysicsServer

open Fodot
open Godot

type PhysicsServer2D with
    static member BodyGetTransform(body : Rid) =
        PhysicsServer2D.BodyGetState(body, PhysicsServer2D.BodyState.Transform)
        |> Variant.toType<Transform2D>
    
    static member BodySetTransform(body : Rid, transform : Transform2D) =
        PhysicsServer2D.BodySetState(body, PhysicsServer2D.BodyState.Transform, transform)
    
type PhysicsServer3D with
    static member BodyGetTransform(body : Rid) =
        PhysicsServer3D.BodyGetState(body, PhysicsServer3D.BodyState.Transform)
        |> Variant.toType<Transform3D>
        
    static member BodySetTransform(body : Rid, transform : Transform3D) =
        PhysicsServer3D.BodySetState(body, PhysicsServer3D.BodyState.Transform, transform)