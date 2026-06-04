module Moon.Module.Node

open Godot
open Fodot.Core

let getSubBinding (map : WeakMeta<'a>) (creator : Node -> 'a) (node : Node)=
    map |> WeakMeta.getOrAdd node (lazy (
        let sub = new Node()
        let result = creator sub
        node |> Node.bindChild sub
        node |> Node.addChildInternalFront sub
        result
    ))

let whenReady (action : unit -> unit) (node : Node) =
    if node.IsNodeReady() then
        action()
    else
        node.add_Ready (fun _ -> action())