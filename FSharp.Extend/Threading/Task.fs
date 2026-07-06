module FSharp.Threading.Task

open System.Threading.Tasks

let run (action : unit -> 'a) =
    Task.Run(action)

let asUnit (t : Task) =
    task {
        do! t.ConfigureAwait(false)
        ()
    }