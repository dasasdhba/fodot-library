module Fodot.Async.Task

open System.Threading
open System.Threading.Tasks
open Fodot
open Fodot.Bridge

let log<'a> (t : Task<'a>) =
    t.LogBy Logger.pushError
    
let logWith<'a> (ct : CancellationToken) (t : Task<'a>) =
    t.LogBy (Logger.pushError, ct)

let forget (t : Task<'a>) =
    t |> log |> ignore
    
let forgetWith (ct : CancellationToken) (t : Task<'a>) =
    t |> logWith ct |> ignore
