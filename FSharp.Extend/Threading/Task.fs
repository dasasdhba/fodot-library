[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module FSharp.Task

open System.Threading.Tasks

let run (action : unit -> 'a) =
    Task.Run(action)

let asUnit (t : Task) =
    task {
        do! t.ConfigureAwait(false)
        ()
    }
