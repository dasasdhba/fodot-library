module Fodot.Module.Color

open Godot
    
let withR (v : float32) (c: Color) =
    let mutable r = c
    r.R <- v
    r
    
let withG (v : float32) (c: Color) =
    let mutable r = c
    r.G <- v
    r
    
let withB (v : float32) (c: Color) =
    let mutable r = c
    r.B <- v
    r
    
let withA (v : float32) (c: Color) =
    let mutable r = c
    r.A <- v
    r
    
let withRGB (rgb : Vector3) (c: Color) =
    let mutable r = c
    r.R <- rgb.X
    r.G <- rgb.Y
    r.B <- rgb.Z
    r