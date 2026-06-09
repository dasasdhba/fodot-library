namespace Moon.Library

open Fodot.Core
open Godot

type PhysicsState2D(node : Node) =
    
    let mutable state : PhysicsDirectSpaceState2D option = None
    
    let updateSpace () =
        state <-
            node.GetViewport() |> Option.ofObj
            |> Option.bind (fun v -> v.FindWorld2D() |> Option.ofObj)
            |> Option.map _.DirectSpaceState
    
    do
        node.add_TreeEntered updateSpace
        if node.IsInsideTree() then updateSpace ()
    
    static let map = WeakMeta<PhysicsState2D>()
    static member Get(node : Node) =
        map |> WeakMeta.getOrAdd node (lazy PhysicsState2D(node))
    
    member val SpaceState = state
    
type PhysicsState3D(node : Node) =
    
    let mutable state : PhysicsDirectSpaceState3D option = None
    
    let updateSpace () =
        state <-
            node.GetViewport() |> Option.ofObj
            |> Option.bind (fun v -> v.FindWorld3D() |> Option.ofObj)
            |> Option.map _.DirectSpaceState
    
    do
        node.add_TreeEntered updateSpace
        if node.IsInsideTree() then updateSpace ()
    
    static let map = WeakMeta<PhysicsState3D>()
    static member Get(node : Node) =
        map |> WeakMeta.getOrAdd node (lazy PhysicsState3D(node))
    
    member val SpaceState = state
