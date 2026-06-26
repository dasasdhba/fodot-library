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
        
    let from (v : Vector2I) =
        Vector2(v.X |> float32, v.Y |> float32)
        
module Vector2I =
    
    let withX (x : int) (v : Vector2I) =
        let mutable r = v
        r.X <- x
        r
        
    let withY (y : int) (v : Vector2I) =
        let mutable r = v
        r.Y <- y
        r
        
    let from (v : Vector2) =
        Vector2I(v.X |> int, v.Y |> int)
        
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
        
    let from (v : Vector3I) =
        Vector3(v.X |> float32, v.Y |> float32, v.Z |> float32)
        
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
    
    let from (v : Vector3) =
        Vector3I(v.X |> int, v.Y |> int, v.Z |> int)
        
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
        
    let from (v : Vector4I) =
        Vector4(v.X |> float32, v.Y |> float32, v.Z |> float32, v.W |> float32)
        
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
        
    let from (v : Vector4) =
        Vector4I(v.X |> int, v.Y |> int, v.Z |> int, v.W |> int)