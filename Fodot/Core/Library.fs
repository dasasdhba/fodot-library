namespace Fodot.Core

open Godot

type GDLib(path : string) =
    static let libName = new StringName "lib"
    let res = GD.load path
    
    member val Resource = res
    member val Lib =
        res |> GodotObject.getAsDictionary<string, Variant> libName
    member this.Get<'a> (key : string) =
        this.Lib[key] |> Variant.toType<'a>