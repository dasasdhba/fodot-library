module Fodot.Extend.TileSet

open Godot

let getCustomDataLayerIndex name (tileset : TileSet) =
     [0 .. (tileset.GetCustomDataLayersCount() - 1)]
     |> List.tryFind (fun i -> tileset.GetCustomDataLayerName(i) = name)

let hasCustomData name (tileset : TileSet) =
     tileset
     |> getCustomDataLayerIndex name
     |> Option.isSome
     
let removeCustomData name (tileset : TileSet) =
     tileset
     |> getCustomDataLayerIndex name
     |> Option.map tileset.RemoveCustomDataLayer
     |> Option.isSome