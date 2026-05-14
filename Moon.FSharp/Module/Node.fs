module Moon.Module.Node

open Godot
open Fodot.Stage

let getUniquePath (node : Node) =
    node
    |> Node.tryGetStage
    |> Option.map (fun s -> s.Viewport.GetPathTo(node))
    |> Option.defaultWith (fun _ -> node.GetPath())