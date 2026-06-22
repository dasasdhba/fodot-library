namespace Fodot

open System
open System.Collections.Concurrent
open System.Collections.Frozen
open System.Collections.Generic
open System.Reflection
open FSharp.Builder
open FSharp.Extend
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
                let tag = (attr :?> FScriptAttribute).Tag
                t, tag
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
    
    let private getAttribute<'a> () =
        attrMap.Value[typeof<'a>]
    
    let private typeMap = lazy (
        let dict =
            typeList.Value
            |> List.fold (fun (s: Dictionary<obj, Type list>) (t, tag) ->
                if s.ContainsKey tag then
                    s[tag] <- t :: s[tag]
                else
                    s[tag] <- [t]
                s
            ) (Dictionary<obj, Type list>())
    
    #if DEBUG
        
        let allScripts =
            dict.Keys
            |> Seq.choose (fun tag ->
                match tag with
                | :? string as n -> Some n
                | _ -> None
            )
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
    
    let private tryGetTypes (tag: obj) =
        typeMap.Value |> Dict.tryGetValue tag
    
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

    let private create (tag: obj) (args: obj array) = maybe {
        let! typs = tag |> tryGetTypes

        return typs |> List.choose (fun typ ->
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
    }
    
    type private FScriptData() =
        member val Tags: obj list = [] with get, set
        member val Scripts : obj list = [] with get, set

    let private fScriptTable = WeakMeta<FScriptData>()
    
    let private getScriptData (obj : GodotObject) =
        fScriptTable |> WeakMeta.getOrAdd obj (fun () -> FScriptData())
    
    let private tryGetScriptData (obj : GodotObject) =
        fScriptTable |> WeakMeta.tryGet obj
    
    let private hasScriptData (obj : GodotObject) =
        fScriptTable |> WeakMeta.contains obj
        
    let containsTag (tag: obj) (obj : GodotObject) =
        obj
        |> tryGetScriptData
        |> Option.map (fun data ->
            data.Tags |> List.contains tag
        )
        |> Option.defaultValue false

    let private getMetaAndGroupList (obj : GodotObject) =
        seq {
            yield! obj |> GodotObject.getMetaList
            match obj with
                | :? Node as n -> yield! n.GetGroups ()
                | _ -> ()
        }

        |> Seq.choose (fun m ->
            let s = m |> string
            if s.StartsWith "fs_" && s.Length > 3 then
                Some s[3..]
            else
                None
        )
    
    let private fsProcessIdle = new StringName "_fs_process"
    let private fsProcessPhysics = new StringName "_fs_physics_process"
    let private fsCallbackGd = new StringName "_get_fscripts"
        
    let private getCallbackFScripts (obj : GodotObject) : string seq =
        let getCallArrWith (name : StringName) =
            match obj |> GodotObject.tryInvokeAs<string[]> name with
            | Some arr -> arr
            | None -> Array.empty
        
        let getGdCall () = seq {
             yield! getCallArrWith fsCallbackGd
             if obj.HasMethod fsProcessIdle then
                 if obj.GetMethodArgumentCount fsProcessIdle = 0 then
                     yield "fodot_hack_process"
                 else
                     yield "fodot_process"
             if obj.HasMethod fsProcessPhysics then
                 if obj.GetMethodArgumentCount fsProcessPhysics = 0 then
                     yield "fodot_hack_physics_process"
                 else
                     yield "fodot_physics_process"
        }
        
        if obj :> obj :? IFScripts then
            (obj :> obj :?> IFScripts).GetFScripts ()
        elif obj.GetScript() |> Variant.toSome<GDScript> |> Option.isSome then
            getGdCall ()
        else
            Seq.empty
    
    let private ancestorCache = ConcurrentDictionary<Type, obj array>()
    
    let private getAncestors (typ : Type) =
        let rec get (t : Type) =
            match t.BaseType with
            | null -> t :> obj |> Seq.singleton
            | b -> t :> obj |> Seq.singleton |> Seq.append (b |> get)
        
        ancestorCache.GetOrAdd(typ, fun t ->
            get t |> Array.ofSeq
        )
    
    let update (obj : GodotObject) =
        let arr =
            obj
            |> getMetaAndGroupList
            |> Seq.append (obj |> getCallbackFScripts)
            |> Seq.distinct
            |> Seq.map (fun s -> s :> obj)
            |> Seq.append (obj.GetType() |> getAncestors)
            |> Seq.filter (fun s -> obj |> containsTag s |> not)
        
        let data = getScriptData obj
        let (keys : obj list), objs =
            arr |> Seq.fold (fun (tags, objs) m ->
                match create m [|obj|] with
                | Some scripts ->
                    m :: tags, scripts @ objs
                | None ->
                    if m :? string then
                        Logger.pushError $"{obj}: fscript {m} not found"
                    tags, objs
            ) (data.Tags, data.Scripts)
            
        data.Tags <- keys
        data.Scripts <- objs
            
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
            |> Seq.tryPick (fun s ->
                match s with
                | :? 'a as a -> Some a
                | _ -> None
            )
        )
    
    let get<'a> (obj: GodotObject) =
        obj
        |> tryGet<'a>
        |> Option.defaultWith (fun () -> failwith $"{obj} does not have {typeof<'a>} script.")
        
    let contains<'a> (obj: GodotObject) =
        obj |> tryGet<'a> |> Option.isSome
        
    /// This can be only used to attach FScript.
    /// It will check existing scripts first.
    /// Warning: you should not call this until init.
    let attach<'a> (obj : GodotObject) =
        let attr = getAttribute<'a> ()
        
        if obj |> containsTag attr |> not then
            create attr [|obj|]
            |> Option.iter (fun s ->
                let data = getScriptData obj
                data.Tags <- attr :: data.Tags
                data.Scripts <- s @ data.Scripts
            )
        
        obj |> get<'a>
