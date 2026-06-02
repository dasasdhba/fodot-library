namespace Fodot.Core

open System.Runtime.CompilerServices
open Godot

type WeakMap<'a when 'a : not struct> = ConditionalWeakTable<GodotObject, 'a>

module WeakMap =
    
    let remove obj (weakRef: WeakMap<'a>) =
        weakRef.Remove obj
    
    let addOrUpdate obj value (weakRef: WeakMap<'a>) =
        weakRef.AddOrUpdate(obj, value)
    
    let getOrAdd obj (value : Lazy<'a>) (weakRef: WeakMap<'a>) =
        match weakRef.TryGetValue obj with
        | true, value -> value
        | false, _ ->
            weakRef.Add(obj, value.Value)
            value.Value
            
    let tryGet obj (weakRef: WeakMap<'a>) =
        match weakRef.TryGetValue obj with
        | true, value -> Some value
        | false, _ -> None
        
    let get obj (weakRef: WeakMap<'a>) =
        weakRef
        |> tryGet obj
        |> Option.defaultWith (fun _ -> failwith $"{weakRef}: Obj {obj} not found.")
        
    let contains obj (weakRef: WeakMap<'a>) =
        weakRef
        |> tryGet obj
        |> Option.isSome