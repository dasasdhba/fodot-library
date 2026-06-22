module Fodot.Logger

open Godot

let private getPrefix () =
    if GodotThread.IsMainThread() then
        if Engine.IsInPhysicsFrame() then
            $"[Physics Frame {Engine.GetPhysicsFrames()}]"
        else
            $"[Idle Frame {Engine.GetProcessFrames()}]"
    else
        $"[Thread {System.DateTime.Now}]"

let push message =
    GD.Print $"{getPrefix()} {message}"
    
let pushWarn message =
    GD.PushWarning $"{getPrefix()} {message}"
    
let pushError message =
    GD.PushError $"{getPrefix()} {message}"
