module Fodot.Core.GDThread

open Godot

/// post an action to Godot main thread without blocking current thread
let post (action: unit -> unit) =
    Dispatcher.SynchronizationContext.Post((fun _ -> action()), null)

/// send an action to Godot main thread and return until it finished
let send (action: unit -> unit) =
    Dispatcher.SynchronizationContext.Send((fun _ -> action()), null)

/// post an action with obj to Godot main thread without blocking current thread
let postBy<'a> (action: 'a -> unit) (obj : 'a) =
    Dispatcher.SynchronizationContext.Post((fun o -> action(o :?> 'a)), obj)

/// send an action with obj to Godot main thread and return until it finished
let sendBy<'a> (action: 'a -> unit) (obj : 'a)=
    Dispatcher.SynchronizationContext.Send((fun o -> action(o :?> 'a)), obj)