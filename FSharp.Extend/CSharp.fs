namespace FSharp

open System
open System.Runtime.CompilerServices
open System.Threading.Tasks
open FSharp.Threading

module OptionExt =
    
    [<Extension>]
    let AsObj (o : 'a option) =
        o |> Option.asObj

module ObservableExt =
    
    [<Extension>]
    let Subscribe (o : IObservable<'a>) (f : Action<'a>) =
        o.Subscribe f.Invoke

module UnitObservableExt =
    
    [<Extension>]
    let Subscribe (o : IObservable<unit>) (f : Action) =
        o.Subscribe f.Invoke

module EventExt =
    
    [<Extension>]
    let Add (e : IEvent<'a>) (f : Action<'a>) =
        e.Add f.Invoke
    
module UnitEventExt =
    
    [<Extension>]
    let Add (e : IEvent<unit>) (f : Action) =
        e.Add f.Invoke

module FuncExt =
    
    [<Extension>]
    let AsFSharpFunc<'a, 'b> (f : Func<'a, 'b>) =
        f.Invoke

module UnitFuncExit =
    
    [<Extension>]
    let AsFSharpFunc<'a> (f : Func<'a>) =
        f.Invoke

module ActionExt =
    
    [<Extension>]
    let AsFSharpFunc<'a> (f : Action<'a>) =
        f.Invoke

module UnitActionExt =
    
    [<Extension>]
    let AsFSharpFunc (f : Action) =
        f.Invoke

module ThreadingExt =

    [<Extension>]
    let AsUnit (t : Task) =
        t |> Task.asUnit