module Moon.Module.Resource

open Fodot
open Godot
open Moon

let findAllBy<'a when 'a :> Resource> (filter : string -> bool) (path : string) =
    let dir = DirAccess.Open path
    dir.GetResourcePathsRecursively(filter)
    |> Seq.choose (fun s ->
        GD.tryLoadAs<'a> s
        |> Option.map (fun c -> s, c)
    )

let findAll<'a when 'a :> Resource> (path : string) =
    findAllBy<'a> (fun _ -> true) path

let findAllTres<'a when 'a :> Resource> (path : string) =
    findAllBy<'a> (fun s -> s.GetExtension() = "tres") path
