module Fodot.Module.GodotObject

open Godot
open Fodot.Core

let tryGetInterface<'a> (obj : GodotObject) =
    try
        Some (obj :> obj :?> 'a)
    with
    | _ -> obj |> FScript.tryGet<'a>
        
let getInterface<'a> (obj : GodotObject) =
    obj
    |> tryGetInterface<'a>
    |> Option.defaultWith (fun () -> failwith $"Object {obj} does not implement interface {typeof<'a>}")
    
let getAllInterface<'a> (obj : GodotObject) = seq {
    try
        yield obj :> obj :?> 'a
    with
    | _ -> ()

    yield! obj |> FScript.getAll<'a>
}