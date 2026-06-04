module Fodot.Async.Event

open System.Threading
open System.Threading.Tasks

// fsharp event

let awaitWith (ct: CancellationToken) (event: IEvent<'Delegate, 'Args>) =
    let tcs = TaskCompletionSource<'Args>()
    
    let subscription = event.Subscribe(fun args -> 
        tcs.TrySetResult(args) |> ignore
    )

    let registration = ct.Register(fun () -> 
        tcs.TrySetCanceled() |> ignore
    )

    task {
        try
            return! tcs.Task
        finally
            subscription.Dispose()
            registration.Dispose()
    }
    
let await (event: IEvent<'Delegate, 'Args>) =
    awaitWith CancellationToken.None event