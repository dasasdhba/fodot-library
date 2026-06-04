namespace Fodot.Core

open System
open System.Collections.Concurrent
open System.Collections.Frozen
open System.Collections.Generic
open System.Reflection
open Fodot.Common
open Godot
    
module FScript =
    
    let mutable private assemblies : Assembly array = [||]
    
    let setAssemblies (a: Assembly array) =
        assemblies <- a
    
    let private initTypes () =
        //AppDomain.CurrentDomain.GetAssemblies()
        assemblies

        |> Seq.collect (fun asm ->
            try asm.GetTypes()
            with _ -> Array.empty
        )

        |> Seq.choose (fun t ->
            t.GetCustomAttributes(typeof<FScriptAttribute>, false)

            |> Array.tryHead
            |> Option.map (fun attr -> 
                let name = (attr :?> FScriptAttribute).Name
                t, name
            )
        )
        
        |> List.ofSeq
    
    let private typeList = lazy initTypes()
    let private attrMap = lazy (
        let dict =
            typeList.Value
            |> dict
        dict.ToFrozenDictionary()
    )
    
    let getAttribute<'a> () =
        attrMap.Value[typeof<'a>]
    
    let private typeMap = lazy (
        let dict =
            typeList.Value
            |> List.fold (fun (s: Dictionary<string, Type list>) (t, name) ->
                if s.ContainsKey name then
                    s[name] <- t :: s[name]
                else
                    s[name] <- [t]
                s
            ) (Dictionary<string, Type list>())
    
    #if DEBUG
        
        let allScripts =
            dict.Keys
            |> Seq.map (fun name ->
                let scripts =
                    dict[name]
                    |> List.map (fun s -> $"- {s.FullName}")
                    |> String.concat "\n"
                $"{name}:\n{scripts}"
            )
            |> String.concat "\n"
        Logger.push $"Loaded scripts: \n{allScripts}"
        
    #endif
        
        dict.ToFrozenDictionary ()
    )
    
    let private getTypes (name: string) =
        typeMap.Value[name]
    
    let private paramMap = lazy (
        let dict =
            typeList.Value
            |> List.map (fun (t, _) -> t, t.GetConstructors())
            |> dict
        dict.ToFrozenDictionary ()
    )
    
    let private getConstructors (t: Type) =
        paramMap.Value[t]
        
    let buildCache () =
        attrMap.Value |> ignore
        typeMap.Value |> ignore
        paramMap.Value |> ignore

    let private create (name: string) (args: obj array) =
        name
        |> getTypes
        
        |> List.choose (fun typ ->
            typ
            |> getConstructors
            |> Array.tryHead
            
            // we don't really need multiple constructors
            (*|> Array.tryFind (fun ctor ->
                let parameters = ctor.GetParameters()

                parameters.Length = args.Length &&
                Array.forall2 (fun (param: ParameterInfo) arg ->
                    param.ParameterType.IsAssignableFrom(arg.GetType())
                ) parameters args
            )*)
            
            |> Option.map _.Invoke(args)
        )
    
    type private FScriptData() =
        member val Keys = ConcurrentBag<string>() with get
        member val Scripts = ConcurrentBag<Object>() with get

    let private fScriptTable = WeakMeta<FScriptData>()
    
    let private getScriptData (obj : GodotObject) =
        fScriptTable |> WeakMeta.getOrAdd obj (lazy FScriptData())
    
    let private tryGetScriptData (obj : GodotObject) =
        fScriptTable |> WeakMeta.tryGet obj
    
    let private hasScriptData (obj : GodotObject) =
        fScriptTable |> WeakMeta.contains obj
    
    let private updateScriptData (name : string) (scripts : Object list) (obj : GodotObject) =
        let data = obj |> getScriptData
        data.Keys.Add name
        scripts |> List.iter (fun s -> data.Scripts.Add s)
        
    let containsKey (name : string) (obj : GodotObject) =
        obj
        |> tryGetScriptData
        |> Option.map (fun data ->
            data.Keys |> Seq.contains name
        )
        |> Option.defaultValue false

    let private getMetaAndGroupList (obj : GodotObject) =
        obj |> GodotObject.getMetaList
        |> List.ofSeq

        |> List.append (
            match obj with
            | :? Node as n -> n.GetGroups () |> List.ofSeq
            | _ -> []
        )

        |> List.choose (fun m ->
            let s = m |> string
            if s.StartsWith "fs_" && s.Length > 3 then
                Some s[3..]
            else
                None
        )
        
    let private fsCallbackGd = new StringName "_get_fscripts"
    let private fsCallbackCs = new StringName "_GetFScripts"
        
    let private getCallbackFScripts (obj : GodotObject) =
        let getCallArrWith (name : StringName) =
            match obj |> GodotObject.tryInvokeAs<string[]> name with
            | Some arr -> arr |> List.ofSeq
            | None -> []
        
        getCallArrWith fsCallbackGd
        |> List.append (getCallArrWith fsCallbackCs)

    let update (obj : GodotObject) =
        let arr =
            obj
            |> getMetaAndGroupList
            |> List.append (obj |> getCallbackFScripts)
            |> List.distinct
            |> List.filter (fun s -> obj |> containsKey s |> not)
        
        for m in arr do
            try
                let scripts = create m [|obj|]
                obj |> updateScriptData m scripts
            with
            
            | ex -> Logger.pushError $"{obj}: failed creating script {m}: {ex}"
            
    let init (obj : GodotObject) =
        if obj |> hasScriptData then
            ()
        else
            obj |> update
    
    let getAll<'a> (obj : GodotObject) =
        obj
        |> tryGetScriptData
        |> Option.map (fun data ->
            data.Scripts
            |> Seq.choose (fun s ->
                match s with
                | :? 'a as a -> Some a
                | _ -> None
            )
        )
        |> Option.defaultValue Seq.empty
    
    let tryGet<'a> (obj : GodotObject) =
        obj
        |> tryGetScriptData
        |> Option.bind (fun data ->
            data.Scripts
            |> Seq.tryFind (fun s -> s :? 'a)
            |> Option.map (fun s -> s :?> 'a)
        )
    
    let get<'a> (obj: GodotObject) =
        obj
        |> tryGet<'a>
        |> Option.defaultWith (fun () -> failwith $"{obj} does not have {typeof<'a>} script.")
        
    let contains<'a> (obj: GodotObject) =
        obj |> tryGet<'a> |> Option.isSome
        
    /// This can be only used to attach FScript.
    /// It will check existing script first.
    /// Warning: you should not call this until init.
    let attach<'a> (obj : GodotObject) =
        let attr = getAttribute<'a> ()
        
        if obj |> containsKey attr |> not then
            obj |> GodotObject.setMeta (new StringName $"fs_{attr}") true
            obj |> update
        
        obj |> get<'a>