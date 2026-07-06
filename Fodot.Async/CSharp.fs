namespace Fodot.CSharp

open System.Runtime.CompilerServices
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