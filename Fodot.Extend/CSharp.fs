module Fodot.CSharp.ExtendExtensions

open System.Runtime.CompilerServices
open Fodot.Extend
open Godot

[<Extension>]
let GetOwnerOrSelf (node : Node) =
    node |> Node.getOwnerOrSelf
    
[<Extension>]
let LoadAs<'a when 'a :> Resource> (node : Node) (path : string) =
    node |> Node.loadAs<'a> path
    
[<Extension>]
let LoadAsOrNull<'a when 'a : null and 'a :> Resource> (node : Node) (path : string) =
    match node |> Node.tryLoadAs<'a> path with
    | Some resource -> resource
    | None -> null