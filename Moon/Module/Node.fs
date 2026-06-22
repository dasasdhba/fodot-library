module Moon.Module.Node

open Godot
open Fodot

let getSubBindingInternal (map : WeakMeta<'a>) inter (creator : Node -> 'a) (node : Node)=
    map |> WeakMeta.getOrAdd node (fun () ->
        let sub = new Node()
        let result = creator sub
        node |> Node.bindChild sub
        node |> Node.addChildInternal sub inter
        result
    )

let getSubBindingFront (map : WeakMeta<'a>) (creator : Node -> 'a) (node : Node)=
    node |> getSubBindingInternal map Node.InternalMode.Front creator
    
let getSubBindingBack (map : WeakMeta<'a>) (creator : Node -> 'a) (node : Node)=
    node |> getSubBindingInternal map Node.InternalMode.Back creator

let whenReady (action : unit -> unit) (node : Node) =
    if node.IsNodeReady() then
        action()
    else
        node.add_Ready (fun _ -> action())
