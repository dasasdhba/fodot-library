module Fodot.Core.GodotObject

open System
open System.Collections.Concurrent
open Fodot.Common
open Godot
open Microsoft.FSharp.Reflection

// metadata

/// will return false if metadata value is null,
/// if this is unexpected, use Godot's HasMeta instead.
let hasMeta (name : StringName) (obj : GodotObject) =
    obj.HasMeta(name) && obj.GetMeta(name).VariantType <> Variant.Type.Nil

let setMeta (name : StringName) (var : 'a) (obj : GodotObject) =
    obj.SetMeta(name, var |> Variant.from)

/// will fail if metadata value is null,
/// if this is unexpected, use Godot's GetMeta instead.
let getMeta (name : StringName) (obj : GodotObject) =
    if obj.HasMeta name |> not then
        failwith $"{obj}: Meta {name} not found."
    else
        match obj.GetMeta(name) with
        | v when v.VariantType = Variant.Type.Nil ->
            failwith $"{obj}: Meta {name} contains a null value."
        | v -> v

let getMetaAs<'a> (name : StringName) (obj : GodotObject) =
    obj |> getMeta name |> Variant.toType<'a>
    
let getMetaAsArray<'a> (name : StringName) (obj : GodotObject) =
    obj |> getMeta name |> Variant.toArray<'a>
    
let getMetaAsDictionary<'a, 'b> (name : StringName) (obj : GodotObject) =
    obj |> getMeta name |> Variant.toDictionary<'a, 'b>

/// will return None if metadata value is null,
let tryGetMeta (name : StringName) (obj : GodotObject) =
    if obj.HasMeta name |> not then
        None
    else
        match obj.GetMeta(name) with
        | v when v.VariantType = Variant.Type.Nil ->
            None
        | v -> Some v
    
let tryGetMetaAs<'a> (name : StringName) (obj : GodotObject) =
    obj |> tryGetMeta name |> Option.bind (fun r -> r |> Variant.toSome<'a>)
    
let tryGetMetaAsArray<'a> (name : StringName) (obj : GodotObject) =
    obj |> tryGetMeta name |> Option.bind (fun r -> r |> Variant.toSomeArray<'a>)

let tryGetMetaAsDictionary<'a, 'b> (name : StringName) (obj : GodotObject) =
    obj |> tryGetMeta name |> Option.bind (fun r -> r |> Variant.toSomeDictionary<'a, 'b>)

let removeMeta (name : StringName) (obj : GodotObject) =
    if obj |> hasMeta name then
        obj.RemoveMeta(name)
        true
    else
        false
    
let getMetaList (obj : GodotObject) =
    obj.GetMetaList()
    
let private getDefaultMetaWith<'a> getter (name : StringName) (def : Lazy<'a>) (obj : GodotObject) =
    obj
    |> getter name
    |> Option.defaultWith (fun () ->
        obj |> setMeta name def.Value
        def.Value
    )
    
let getMetaWithDefaultAs<'a> (name : StringName) (def : Lazy<'a>) (obj : GodotObject) =
    obj |> getDefaultMetaWith tryGetMetaAs name def
        
let getMetaWithDefaultAsArray<'a> (name : StringName) (def : Lazy<Collections.Array<'a>>) (obj : GodotObject) =
    obj |> getDefaultMetaWith tryGetMetaAsArray name def
        
let getMetaWithDefaultAsDictionary<'a, 'b> (name : StringName) (def : Lazy<Collections.Dictionary<'a, 'b>>) (obj : GodotObject) =
    obj |> getDefaultMetaWith tryGetMetaAsDictionary name def
    
// property get set

let private createPropertyList (obj : GodotObject) =
    obj.GetPropertyList()

    |> Array.ofSeq
    |> Array.map (fun p -> p["name"] |> Variant.toType<StringName>)

let private propMeta = new StringName "_fs_GodotObject_prop_list"

let getPropertyList (obj : GodotObject) =
    obj |> getMetaWithDefaultAs propMeta (lazy createPropertyList obj)

let hasProperty (prop : StringName) (obj : GodotObject) =
    obj |> getPropertyList |> Array.contains prop

let get (prop : StringName) (obj : GodotObject) =
    obj.Get(prop)

let getAs<'a> (prop : StringName) (obj : GodotObject) =
    obj |> get prop |> Variant.toType<'a>
    
let getAsArray<'a> (prop : StringName) (obj : GodotObject) =
    obj |> get prop |> Variant.toArray<'a>
    
let getAsDictionary<'a, 'b> (prop : StringName) (obj : GodotObject) =
    obj |> get prop |> Variant.toDictionary<'a, 'b>
    
let tryGet (prop : StringName) (obj : GodotObject) =
    match obj.Get(prop) with
    | v when v.VariantType = Variant.Type.Nil ->
        None
    | v -> Some v
    
let tryGetAs<'a> (prop : StringName) (obj : GodotObject) =
    obj |> tryGet prop |> Option.bind (fun r -> r |> Variant.toSome<'a>)
    
let tryGetAsArray<'a> (prop : StringName) (obj : GodotObject) =
    obj |> tryGet prop |> Option.bind (fun r -> r |> Variant.toSomeArray<'a>)
    
let tryGetAsDictionary<'a, 'b> (prop : StringName) (obj : GodotObject) =
    obj |> tryGet prop |> Option.bind (fun r -> r |> Variant.toSomeDictionary<'a, 'b>)

let set (prop : StringName) (value : 'a) (obj : GodotObject) =
    if obj |> hasProperty prop |> not then
        failwith $"{obj}: Property {prop} not found."
    else
        obj.Set(prop, value |> Variant.from)
        
// method

let hasMethod (method : StringName) (obj : GodotObject) =
    obj.HasMethod(method)
    
let call<'a> (method : StringName) (args : 'a) (obj : GodotObject) =
    if obj |> hasMethod method |> not then
        failwith $"{obj}: Method {method} not found."
    else
        obj.Call (method, args |> Variant.fromTuple)
    
let callDeferred<'a> (method : StringName) (args : 'a) (obj : GodotObject) =
    if obj |> hasMethod method |> not then
        failwith $"{obj}: Method {method} not found."
    else
        obj.CallDeferred (method, args |> Variant.fromTuple) |> ignore

let tryCall<'a> (method : StringName) (args : 'a) (obj : GodotObject) =
    if obj |> hasMethod method |> not then
        None
    else
        obj.Call (method, args |> Variant.fromTuple) |> Some

let callAs<'a, 'b> (method : StringName) (args : 'a) (obj : GodotObject) =
    obj |> call<'a> method args |> Variant.toType<'b>

let callAsArray<'a, 'b> (method : StringName) (args : 'a) (obj : GodotObject) =
    obj |> call<'a> method args |> Variant.toArray<'b>

let callAsDictionary<'a, 'b, 'c> (method : StringName) (args : 'a) (obj : GodotObject) =
    obj |> call<'a> method args |> Variant.toDictionary<'b, 'c>

let tryCallAs<'a, 'b> (method : StringName) (args : 'a) (obj : GodotObject) =
    obj |> tryCall<'a> method args |> Option.bind (fun r -> r |> Variant.toSome<'b>)

let tryCallAsArray<'a, 'b> (method : StringName) (args : 'a) (obj : GodotObject) =
    obj |> tryCall<'a> method args |> Option.bind (fun r -> r |> Variant.toSomeArray<'b>)

let tryCallAsDictionary<'a, 'b, 'c> (method : StringName) (args : 'a) (obj : GodotObject) =
    obj |> tryCall<'a> method args |> Option.bind (fun r -> r |> Variant.toSomeDictionary<'b, 'c>)

let invoke (method : StringName) (obj : GodotObject) =
    obj |> call<unit> method ()

let invokeDeferred (method : StringName) (obj : GodotObject) =
    obj |> callDeferred method ()

let tryInvoke (method : StringName) (obj : GodotObject) =
    obj |> tryCall<unit> method ()

let invokeAs<'a> (method : StringName) (obj : GodotObject) =
    obj |> callAs<unit, 'a> method ()

let invokeAsArray<'a> (method : StringName) (obj : GodotObject) =
    obj |> callAsArray<unit, 'a> method ()

let invokeAsDictionary<'a, 'b> (method : StringName) (obj : GodotObject) =
    obj |> callAsDictionary<unit, 'a, 'b> method ()

let tryInvokeAs<'a> (method : StringName) (obj : GodotObject) =
    obj |> tryCallAs<unit, 'a> method ()

let tryInvokeAsArray<'a> (method : StringName) (obj : GodotObject) =
    obj |> tryCallAsArray<unit, 'a> method ()
    
let tryInvokeAsDictionary<'a, 'b> (method : StringName) (obj : GodotObject) =
    obj |> tryCallAsDictionary<unit, 'a, 'b> method ()

// record

let private cache =
    ConcurrentDictionary<Type, string array>()

/// convert godot obj's property to readonly record.
/// cannot handle typed Godot Array or Dictionary, using variant one in record instead.
let deserialize<'T when 'T : not struct> (obj: GodotObject) : 'T =
    let recordType = typeof<'T>
    
    if not (FSharpType.IsRecord recordType) then
        failwith $"{recordType.Name} is not a valid F# Record."
    
    let props =
        cache.GetOrAdd(recordType, (fun r ->
            r
            |> FSharpType.GetRecordFields
            
            |> Array.map (fun prop ->
                prop.GetCustomAttributes(typeof<GDProperty>, false)
                |> Array.tryHead
                |> Option.map (fun attr -> 
                    (attr :?> GDProperty).Name
                )
                |> Option.defaultValue prop.Name
            )
        ))
    
    let values =
        props
        
        |> Array.map (fun prop ->
            let variant = obj.Get(prop)
            variant.Obj |> box
        )
    
    FSharpValue.MakeRecord(recordType, values) :?> 'T