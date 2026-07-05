[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Fodot.PackedScene

open Godot

let instantiateWith gen (packedScene: PackedScene) =
    let node = 
        lock GD.loadLock (fun () ->
            packedScene.Instantiate(gen)
        )
    node |> Node.initScripts
    node
    
let instantiate (packedScene: PackedScene)  =
    let node = 
        lock GD.loadLock (fun () ->
            packedScene.Instantiate()
        )
    node |> Node.initScripts
    node
    
let instantiateToWith<'a when 'a :> Node> gen (packedScene: PackedScene)=
    let node = packedScene |> instantiateWith gen
    node :?> 'a
    
let instantiateTo<'a when 'a :> Node> (packedScene: PackedScene) =
    let node = packedScene |> instantiate
    node :?> 'a
