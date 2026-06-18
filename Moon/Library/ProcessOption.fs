namespace Moon.Library

open Fodot
open Fodot.Extend
open Godot

type ProcessOption<'a> = ProcessFunc<'a option>

module ProcessOption =
    
    let bind (node : Node) (proc : ProcessOption<'a>) : ProcessOption<'a> =
        Delta (fun delta ->
            node
            |> GodotObject.validate
            |> Option.filter _.IsInsideTree()
            |> Option.bind (fun _ ->
                proc.Invoke delta
            )
        )

