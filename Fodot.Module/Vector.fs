namespace Fodot.Module

open Godot

module Vector2 =
    
    let withX (x : float32) (v : Vector2) =
        let mutable r = v
        r.X <- x
        r
        
    let withY (y : float32) (v : Vector2) =
        let mutable r = v
        r.Y <- y
        r
        
module Vector2I =
    
    let withX (x : int) (v : Vector2I) =
        let mutable r = v
        r.X <- x
        r
        
    let withY (y : int) (v : Vector2I) =
        let mutable r = v
        r.Y <- y
        r
        
module Vector3 =
    
    let withX (x : float32) (v : Vector3) =
        let mutable r = v
        r.X <- x
        r
        
    let withY (y : float32) (v : Vector3) =
        let mutable r = v
        r.Y <- y
        r
        
    let withZ (z : float32) (v : Vector3) =
        let mutable r = v
        r.Z <- z
        r
        
module Vector3I =
    
    let withX (x : int) (v : Vector3I) =
        let mutable r = v
        r.X <- x
        r
        
    let withY (y : int) (v : Vector3I) =
        let mutable r = v
        r.Y <- y
        r
        
    let withZ (z : int) (v : Vector3I) =
        let mutable r = v
        r.Z <- z
        r
        
module Vector4 =
    
    let withX (x : float32) (v : Vector4) =
        let mutable r = v
        r.X <- x
        r
        
    let withY (y : float32) (v : Vector4) =
        let mutable r = v
        r.Y <- y
        r
        
    let withZ (z : float32) (v: Vector4) =
        let mutable r = v
        r.Z <- z
        r
        
    let withW (w : float32) (v : Vector4) =
        let mutable r = v
        r.W <- w
        r
        
module Vector4I =
    
    let withX (x : int) (v : Vector4I) =
        let mutable r = v
        r.X <- x
        r
        
    let withY (y : int) (v : Vector4I) =
        let mutable r = v
        r.Y <- y
        r
        
    let withZ (z : int) (v : Vector4I) =
        let mutable r = v
        r.Z <- z
        r
        
    let withW (w : int) (v : Vector4I) =
        let mutable r = v
        r.W <- w
        r