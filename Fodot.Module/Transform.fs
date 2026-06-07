namespace Fodot.Module

open Godot

module Transform2D =
    
    let withOrigin (origin: Vector2) (transform: Transform2D) =
        let mutable result = transform
        result.Origin <- origin
        result
        
    let withX (x: Vector2) (transform: Transform2D) =
        let mutable result = transform
        result.X <- x
        result
    
    let withY (y: Vector2) (transform: Transform2D) =
        let mutable result = transform
        result.Y <- y
        result
            
    let withRotation (rotation: float32) (transform: Transform2D) =
        Transform2D(rotation, transform.Scale, transform.Skew, transform.Origin)
        
    let withScale (scale: Vector2) (transform: Transform2D) =
        Transform2D(transform.Rotation, scale, transform.Skew, transform.Origin)
        
    let withSkew (skew: float32) (transform: Transform2D) =
        Transform2D(transform.Rotation, transform.Scale, skew, transform.Origin)
        
module Transform3D =
    
    let withOrigin (origin: Vector3) (transform: Transform3D) =
        let mutable result = transform
        result.Origin <- origin
        result
        
    let withBasis (basis : Basis) (transform: Transform3D) =
        let mutable result = transform
        result.Basis <- basis
        result