module Fodot.Core.GDTask

open Godot

/// post an action to Godot main thread without blocking current thread
let post<'a when 'a :> GodotObject> (action: 'a -> unit) (obj : 'a) =
    Dispatcher.SynchronizationContext.Post((fun o -> action(o :?> 'a)), obj)

/// send an action to Godot main thread and return until it finished
let send<'a when 'a :> GodotObject> (action: 'a -> unit) (obj : 'a) =
    Dispatcher.SynchronizationContext.Send((fun o -> action(o :?> 'a)), obj)