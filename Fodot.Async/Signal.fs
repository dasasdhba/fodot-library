module Fodot.Async.Signal

open System.Threading
open Fodot.Async.GD
open Fodot.Core

// godot signal

let awaitWith<'a> (ct: CancellationToken) signal obj =
    let signal : GDSignal<'a> = {
        Object = obj
        SignalName = signal
    }
    signal.AsTask ct
    
let await<'a> signal obj =
    awaitWith CancellationToken.None signal obj