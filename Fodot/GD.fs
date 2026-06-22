namespace Fodot

open System
open System.Collections.Concurrent
open FSharp.Concurrent
open Godot
open Godot.Collections

/// A wrapper for GodotObject's property.
/// May fail if the property does not exist.
type GDProp<'a> =
    {
        Object : GodotObject
        PropName : StringName
    }
    member this.Get () =
        this.Object |> GodotObject.getAs<'a> this.PropName
    member this.Set (value : 'a) =
        this.Object |> GodotObject.set this.PropName value
    
    static member New<'a> (prop : StringName) (obj : GodotObject) : GDProp<'a> =
        {
            Object = obj
            PropName = new StringName(prop)
        }
    static member From<'a> (prop : string) (obj : GodotObject) : GDProp<'a> =
        {
            Object = obj
            PropName = new StringName(prop)
        }

/// A wrapper for GodotObject's property which allows null value.
/// May fail if the property does not exist.
type GDNullProp<'a when 'a : null> =
    {
        Object : GodotObject
        PropName : StringName
    }
    member this.Get () =
        this.Object |> GodotObject.tryGetAs<'a> this.PropName
    member this.Set (value : 'a option) =
        match value with
        | Some v -> this.Object |> GodotObject.set this.PropName v
        | None -> this.Object |> GodotObject.set this.PropName null
    
    static member New<'a> (prop : StringName) (obj : GodotObject) : GDNullProp<'a> =
        {
            Object = obj
            PropName = prop
        }
    static member From<'a> (prop : string) (obj : GodotObject) : GDNullProp<'a> =
        {
            Object = obj
            PropName = new StringName(prop)
        }

/// A wrapper for GodotObject's array property.
/// May fail if the property does not exist.
type GDPropArray<'a> =
    {
        Object : GodotObject
        PropName : StringName
    }
    member this.Get () =
        this.Object |> GodotObject.getAsArray<'a> this.PropName
    member this.Set (value : Array<'a>) =
        this.Object |> GodotObject.set this.PropName value
    
    static member New<'a> (prop : StringName) (obj : GodotObject) : GDPropArray<'a> =
        {
            Object = obj
            PropName = prop
        }
    static member From<'a> (prop : string) (obj : GodotObject) : GDPropArray<'a> =
        {
            Object = obj
            PropName = new StringName(prop)
        }

/// A wrapper for GodotObject's dictionary property.
/// May fail if the property does not exist.   
type GDPropDictionary<'a, 'b> =
    {
        Object : GodotObject
        PropName : StringName
    }
    member this.Get () =
        this.Object |> GodotObject.getAsDictionary<'a,'b> this.PropName
    member this.Set (value : Dictionary<'a,'b>) =
        this.Object |> GodotObject.set this.PropName value
    
    static member New<'a, 'b> (prop : StringName) (obj : GodotObject) : GDPropDictionary<'a, 'b> =
        {
            Object = obj
            PropName = prop
        }
    static member From<'a, 'b> (prop : string) (obj : GodotObject) : GDPropDictionary<'a, 'b> =
        {
            Object = obj
            PropName = new StringName(prop)
        }

/// A wrapper for GodotObject's signal.
/// It implements IEvent, it's recommended to use `IObserver` API.
type GDSignal<'a> =
    {
        Object : GodotObject
        SignalName : StringName
        Handlers : ConcurrentDictionary<Handler<'a>, Callable>
    }
    
    static member New (signal : StringName) (obj : GodotObject) : GDSignal<'a> =
        {
            Object = obj
            SignalName = signal
            Handlers = ConcurrentDictionary<Handler<'a>, Callable>()
        }
    static member From (signal : string) (obj : GodotObject) : GDSignal<'a> =
        {
            Object = obj
            SignalName = new StringName(signal)
            Handlers = ConcurrentDictionary<Handler<'a>, Callable>()
        }
    
    member this.ConnectWithFlag (call : Callable) (flags : GodotObject.ConnectFlags) =
        this.Object.Connect(
            this.SignalName,
            call,
            uint32 flags
        )
        
    member this.Connect (call : Callable) =
        this.Object.Connect(
            this.SignalName,
            call
        )
        
    member this.Disconnect (call : Callable)=
        this.Object.Disconnect(
            this.SignalName,
            call
        )
    
    member this.IsConnected (call : Callable) =
        this.Object.IsConnected(
            this.SignalName,
            call
        )
            
    member this.Emit (args : 'a) =
        this.Object.EmitSignal(this.SignalName, args |> Variant.fromTuple)
    
    interface IEvent<'a> with
        member this.AddHandler handler =
            if this.Handlers |> Dict.tryAdd handler (fun () ->
                let call = Callable.from(fun a -> handler.Invoke(null, a))
                this.Connect call |> ignore
                call
            ) |> not then
                Logger.pushWarn $"{this.Object}.{this.SignalName}: Handler {handler} already exists!"
        
        member this.RemoveHandler handler =
            this.Handlers
            |> Dict.tryRemove handler
            |> Option.map this.Disconnect
            |> ignore
        
        member this.Subscribe observer =
            let call = Callable.from observer.OnNext
            let disconnect = this.Disconnect
            this.Connect call |> ignore
            
            {
                new IDisposable with
                member this.Dispose() =
                    disconnect call
            }
        
[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module GD =
    
    let private loadLock = obj()
    
    let tryLoad path =
        lock loadLock (fun () ->
            GD.Load path
        )
        
        |> Option.ofObj
    
    /// This will not init fscript, as we cannot deal with recursive case,
    /// i.e. we cannot get the subresource. 
    let load (path : string) =
        tryLoad path
        |> Option.defaultWith (fun _ -> failwith $"Failed loading {path} as resource")
        
    let loadAs<'a when 'a :> Resource> (path : string) =
        match load path with
        
        | :? 'a as obj -> obj
        | _ -> failwith $"Failed loading {path} as {typeof<'a>}"
        
    let tryLoadAs<'a when 'a :> Resource> (path : string) =
        try
            loadAs<'a> path |> Some
        with
        | _ -> None
