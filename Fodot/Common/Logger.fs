module Fodot.Common.Logger

open Godot

let push message =
    GD.Print $"[{System.DateTime.Now}] {message}"
    
let pushWarn message =
    GD.PushWarning $"[{System.DateTime.Now}] {message}"
    
let pushError message =
    GD.PushError $"[{System.DateTime.Now}] {message}"