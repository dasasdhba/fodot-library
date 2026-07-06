module Fodot.Async.Task

open System.Threading.Tasks
open Fodot

let log<'a> (t : Task<'a>) =
    task {
        try
            return! t.ConfigureAwait(false)
        with ex ->
            Logger.pushError ex
            return Unchecked.defaultof<'a>
    }

let forget (t : Task<'a>) =
    t |> log |> ignore