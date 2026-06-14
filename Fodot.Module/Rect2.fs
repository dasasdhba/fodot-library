module Fodot.Module.Rect2

open Godot

let withEnd (e : Vector2) (r : Rect2) =
    let mutable r = r
    r.End <- e
    r
    
let withPosition (p : Vector2) (r : Rect2) =
    let mutable r = r
    r.Position <- p
    r
    
let withSize (s : Vector2) (r : Rect2) =
    let mutable r = r
    r.Size <- s
    r