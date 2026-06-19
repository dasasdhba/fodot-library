module Fodot.Module.PhysicsServer

open Fodot
open Godot

type PhysicsServer2D with
    static member BodyGetTransform(body : Rid) =
        PhysicsServer2D.BodyGetState(body, PhysicsServer2D.BodyState.Transform)
        |> Variant.toType<Transform2D>
        
type PhysicsServer3D with
    static member BodyGetTransform(body : Rid) =
        PhysicsServer3D.BodyGetState(body, PhysicsServer3D.BodyState.Transform)
        |> Variant.toType<Transform3D>