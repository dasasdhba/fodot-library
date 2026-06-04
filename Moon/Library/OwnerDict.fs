namespace Moon.Library

open System
open System.Collections.Concurrent
open FSharp.Concurrent
open Fodot.Core
open Fodot.Extend
open Godot

type OwnerDict<'a> = WeakMeta<ConcurrentDictionary<string, 'a>>

module OwnerDict =
    
    let tryGetDict (node : Node) (dict : OwnerDict<'a>) =
        dict |> WeakMeta.tryGet (node |> Node.getOwnerOrSelf)
        
    let getDict (node : Node) (dict : OwnerDict<'a>) =
        dict |> WeakMeta.getOrAdd (node |> Node.getOwnerOrSelf) (lazy 
            ConcurrentDictionary<string, 'a>()
        )
    
    let tryGet node key (dict : OwnerDict<'a>) =
        dict
        |> tryGetDict node
        |> Option.bind (fun d -> d |> Dict.tryGetValue key)
        
    let findAll node (predictor: 'a -> 'b option) (dict : OwnerDict<'a>) =
        dict
        |> tryGetDict node
        |> Option.map (fun d -> d.Values |> Seq.choose predictor)
        |> Option.defaultValue Seq.empty
        
    let findOrAdd (node : Node) (predictor: 'a -> 'b option) (value : Lazy<'a * 'b>) (dict : OwnerDict<'a>)  =
        let dict = dict |> getDict node
        
        dict.Values
        |> Seq.choose predictor
        |> Seq.tryHead
        |> Option.defaultWith (fun _ ->
            let k = Guid.NewGuid().ToString()
            let a, b = value.Value
            dict[k] <- a
            b
        )
        
    let getOrAdd (node : Node) key (predictor: 'a -> 'b option) (value : Lazy<'a * 'b>) (dict : OwnerDict<'a>) =
        let dict = dict |> getDict node
        
        dict
        |> Dict.tryGetValue key
        |> Option.bind predictor
        |> Option.defaultWith (fun _ ->
            let a, b = value.Value
            dict[key] <- a
            b
        )