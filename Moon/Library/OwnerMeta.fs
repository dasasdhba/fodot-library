namespace Moon.Library

open System
open Fodot.Core
open Fodot.Extend
open Godot

type OwnerMeta<'a> = WeakMeta<(string * 'a) list>

module OwnerMeta =
    
    let tryGetDict (node : Node) (dict : OwnerMeta<'a>) =
        dict |> WeakMeta.tryGet (node |> Node.getOwnerOrSelf)
        
    let getDict (node : Node) (dict : OwnerMeta<'a>) =
        dict |> WeakMeta.getOrAdd (node |> Node.getOwnerOrSelf) (lazy [])
    
    let updateDict (node : Node) list (dict : OwnerMeta<'a>) =
        dict |> WeakMeta.addOrUpdate (node |> Node.getOwnerOrSelf)
                (lazy list)
                (fun old -> list @ old)
    
    let tryGet node key (dict : OwnerMeta<'a>) =
        dict
        |> tryGetDict node
        |> Option.bind (fun d -> d |> List.tryFind (fun (k, _) -> k = key))
        |> Option.map snd
        
    let findAll node (predictor: 'a -> 'b option) (dict : OwnerMeta<'a>) =
        dict
        |> tryGetDict node
        |> Option.map (fun d -> d |> List.choose (fun (_, a) -> predictor a))
        |> Option.defaultValue List.empty
        
    let findOrAdd (node : Node) (predictor: 'a -> 'b option) (value : Lazy<'a * 'b>) (dict : OwnerMeta<'a>)  =
        dict
        |> getDict node
        |> List.choose (fun (_, a) -> predictor a)
        |> List.tryHead
        |> Option.defaultWith (fun _ ->
            let k = Guid.NewGuid().ToString()
            let a, b = value.Value
            dict |> updateDict node [k, a]
            b
        )
        
    let getOrAdd (node : Node) key (predictor: 'a -> 'b option) (value : Lazy<'a * 'b>) (dict : OwnerMeta<'a>) =
        dict
        |> getDict node
        |> List.tryFind (fun (k, _) -> k = key)
        |> Option.bind (fun (_, a) -> predictor a)
        |> Option.defaultWith (fun _ ->
            let a, b = value.Value
            dict |> updateDict node [key, a]
            b
        )