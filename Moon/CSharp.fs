namespace Moon.CSharp

open System.Runtime.CompilerServices
open Godot
open Moon.Module

module FlipExtensions =
    
    [<Extension>]
    let InitFlipH (node : Node) =
        node |> Flip.initH
    
    [<Extension>]
    let InitFlipV (node : Node) =
        node |> Flip.initV
        
    [<Extension>]
    let TrySetFlipH (node : Node) (value : bool) =
        node |> Flip.setH value
        
    [<Extension>]
    let TrySetFlipV (node : Node) (value : bool) =
        node |> Flip.setV value
        
    [<Extension>]
    let TryGetFlipH (node : Node) =
        node |> Flip.getH
        
    [<Extension>]
    let TryGetFlipV (node : Node) =
        node |> Flip.getV