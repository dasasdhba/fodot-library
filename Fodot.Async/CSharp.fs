namespace Fodot.CSharp

open System.Runtime.CompilerServices
open System.Threading
open System.Threading.Tasks
open FSharp.Threading
open Fodot.Async

module TaskExt =
    
    [<Extension>]
    let Log (t: Task) =
        t |> Task.asUnit |> Task.log
    
    [<Extension>]
    let Forget (t: Task) =
        t |> Task.asUnit |> Task.forget
        
module TaskGenericExt =
    
    [<Extension>]
    let Log (t: Task<'T>) =
        t |> Task.log
    
    [<Extension>]
    let Forget (t: Task<'T>) =
        t |> Task.forget
        
module CtTaskExt =
    
    [<Extension>]
    let Log (t: Task) (ct : CancellationToken) =
        t |> Task.asUnit |> Task.logWith ct
    
    [<Extension>]
    let Forget (t: Task) (ct : CancellationToken) =
        t |> Task.asUnit |> Task.forgetWith ct
        
module CtTaskGenericExt =
    
    [<Extension>]
    let Log (t: Task<'T>) (ct : CancellationToken) =
        t |> Task.logWith ct
    
    [<Extension>]
    let Forget (t: Task<'T>) (ct : CancellationToken) =
        t |> Task.forgetWith ct