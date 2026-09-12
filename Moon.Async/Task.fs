[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Moon.Task

open System.Threading
open System.Threading.Tasks
open Moon
open Moon.Bridge

let log<'a> (t : Task<'a>) =
    t.LogBy Logger.pushError

let logWith<'a> (ct : CancellationToken) (t : Task<'a>) =
    t.LogBy (Logger.pushError, ct)

let forget (t : Task<'a>) =
    t |> log |> ignore

let forgetWith (ct : CancellationToken) (t : Task<'a>) =
    t |> logWith ct |> ignore
