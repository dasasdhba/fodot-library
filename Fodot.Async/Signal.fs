module Fodot.Async.Signal

open System.Threading
open Fodot.Async.GD
open Fodot.Core

// godot signal

let awaitWith<'a> (ct: CancellationToken) signal obj =
    let signal = obj |> GDSignal<'a>.New signal
    signal.AsTask ct
    
let await<'a> signal obj =
    awaitWith<'a> CancellationToken.None signal obj