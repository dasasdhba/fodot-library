[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Moon.Signal

open System.Threading
open Moon

// godot signal

let awaitWith<'a> (ct: CancellationToken) signal obj =
    let signal = obj |> GDSignal<'a>.New signal
    signal.AsTask ct

let await<'a> signal obj =
    awaitWith<'a> CancellationToken.None signal obj
