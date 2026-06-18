namespace Moon.Interface

open Godot

type IPlatformShape3D =
    abstract member OneWayCollision : bool with get, set
    abstract member OneWayCollisionMargin : float32 with get, set
    abstract member OneWayCollisionDirection : Vector3 with get, set