module Moon.Module.Node

open Godot
open Fodot.Core

let getSubBinding (creator : Node -> 'a) (node : Node)=
    node |> FScript.attachBy (lazy (
        let sub = new Node()
        let result = creator sub
        node |> Node.bindChild sub
        node |> Node.addChildInternalFront sub
        result
    ))

let getSubMetaBinding meta (creator : Node -> 'a) (node : Node)=
    node |> GodotObject.getMetaWithDefaultAs<'a> meta (lazy (
        let sub = new Node()
        let result = creator sub
        node |> Node.bindChild sub
        node |> Node.addChildInternalFront sub
        result
    ))

let whenReady (action : unit -> unit) (node : Node) =
    if node.IsInsideTree() then
        action()
    else
        node.add_Ready (fun _ -> action())