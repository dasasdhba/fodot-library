[<AutoOpen>]
module Moon.ExtendExtensions

open System.Runtime.CompilerServices
open FSharp
open Godot

[<Extension>]
let GetOwnerOrSelf (node : Node) =
    node |> Node.getOwnerOrSelf

[<Extension>]
let GetUnique (node : Node) (path : string) =
    node |> Node.getUnique path

[<Extension>]
let LoadAs<'a when 'a :> Resource> (node : Node) (path : string) =
    node |> Node.loadAs<'a> path

[<Extension>]
let LoadAsOrNull<'a when 'a : null and 'a :> Resource> (node : Node) (path : string) =
    node |> Node.tryLoadAs<'a> path |> Option.asObj
