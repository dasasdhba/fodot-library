module Fodot.Async.GDTask

open System.Threading
open GodotTask

// thread control

let runOnThreadWith<'a> ct (f: unit -> 'a) = task {
    let! result = GDTask.RunOnThreadPool(f, false, ct)
    return result
}

let runOnThread<'a> (f: unit -> 'a) =
    runOnThreadWith CancellationToken.None f

let toThreadWith ct physics= task {
    let timing =
        if physics then
            PlayerLoopTiming.IsolatedPhysicsProcess
        else
            PlayerLoopTiming.IsolatedProcess
    do! GDTask.SwitchToMainThread(timing, ct) 
}

let toThread physics =
    toThreadWith CancellationToken.None physics

let toIdleThreadWith ct =
    toThreadWith ct false
    
let toPhysicsThreadWith ct =
    toThreadWith ct true
    
let toIdleThread () =
    toThread false
    
let toPhysicsThread () =
    toThread true