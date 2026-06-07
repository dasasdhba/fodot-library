namespace Fodot.Module

open Godot

module Vector2 =
    
    let withX (v : Vector2) (x : float32) =
        let mutable r = v
        r.X <- x
        r
        
    let withY (v : Vector2) (y : float32) =
        let mutable r = v
        r.Y <- y
        r
        
module Vector2I =
    
    let withX (v : Vector2I) (x : int) =
        let mutable r = v
        r.X <- x
        r
        
    let withY (v : Vector2I) (y : int) =
        let mutable r = v
        r.Y <- y
        r
        
module Vector3 =
    
    let withX (v : Vector3) (x : float32) =
        let mutable r = v
        r.X <- x
        r
        
    let withY (v : Vector3) (y : float32) =
        let mutable r = v
        r.Y <- y
        r
        
    let withZ (v : Vector3) (z : float32) =
        let mutable r = v
        r.Z <- z
        r
        
module Vector3I =
    
    let withX (v : Vector3I) (x : int) =
        let mutable r = v
        r.X <- x
        r
        
    let withY (v : Vector3I) (y : int) =
        let mutable r = v
        r.Y <- y
        r
        
    let withZ (v : Vector3I) (z : int) =
        let mutable r = v
        r.Z <- z
        r
        
module Vector4 =
    
    let withX (v : Vector4) (x : float32) =
        let mutable r = v
        r.X <- x
        r
        
    let withY (v : Vector4) (y : float32) =
        let mutable r = v
        r.Y <- y
        r
        
    let withZ (v: Vector4) (z : float32) =
        let mutable r = v
        r.Z <- z
        r
        
    let withW (v : Vector4) (w : float32) =
        let mutable r = v
        r.W <- w
        r
        
module Vector4I =
    
    let withX (v : Vector4I) (x : int) =
        let mutable r = v
        r.X <- x
        r
        
    let withY (v : Vector4I) (y : int) =
        let mutable r = v
        r.Y <- y
        r
        
    let withZ (v : Vector4I) (z : int) =
        let mutable r = v
        r.Z <- z
        r
        
    let withW (v : Vector4I) (w : int) =
        let mutable r = v
        r.W <- w
        r