namespace Moon.Library

open System.Collections.Generic
open FSharpPlus
open Fodot.Core
open Fodot.Extend
open Godot

type OwnerDict<'a> = WeakMeta<Dictionary<string, 'a>>

module OwnerDict =
    
    let getAll (node : Node) (predictor: 'a -> 'b option) (map : OwnerDict<'a>) : 'b seq =
        map
        |> WeakMeta.tryGet (node |> Node.getOwnerOrSelf)
        |> Option.map (fun dict ->
            dict.Values
            |> Seq.choose predictor
        )
        |> Option.defaultValue Seq.empty

    let tryGet (node : Node) (predictor: 'a -> 'b option) (map : OwnerDict<'a>) =
        map |> getAll node predictor |> Seq.tryHead
        
    let get (node : Node) (predictor: 'a -> 'b option) (map : OwnerDict<'a>) =
        map |> getAll node predictor |> Seq.head

    let tryFind (node : Node) (key : string) (predictor: 'a -> 'b option) (map : OwnerDict<'a>) =
        map
        |> WeakMeta.tryGet (node |> Node.getOwnerOrSelf)
        |> Option.bind (fun d -> d |> Dict.tryGetValue key)
        |> Option.bind predictor

    let find (node : Node) (key : string) (predictor: 'a -> 'b option) (map : OwnerDict<'a>) =
        map
        |> tryFind node key predictor
        |> Option.defaultWith (fun _ -> failwith $"OwnerDict: key {key} not found")
        