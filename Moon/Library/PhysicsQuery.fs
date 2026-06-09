namespace Moon.Library

open Godot
open Moon.Utils

// param

type PhysicsQueryBasicParameters =
    {
        CollideWithAreas : bool
        CollideWithBodies : bool
        CollisionMask : uint32
        Exclude: Rid list
    }
    
    static member New() =
        {
            CollideWithAreas = false
            CollideWithBodies = true
            CollisionMask = 1u
            Exclude = []
        }
    
    static member FromBody(col : CollisionObject2D) =
        {
            CollideWithAreas = false
            CollideWithBodies = true
            CollisionMask = col.CollisionMask
            Exclude = [col.GetRid()]
        }
    
    static member FromBody(col : CollisionObject3D) =
        {
            CollideWithAreas = false
            CollideWithBodies = true
            CollisionMask = col.CollisionMask
            Exclude = [col.GetRid()]
        }
    
    static member From(node : Node) =
        match node with
        | :? CollisionObject2D as col -> PhysicsQueryBasicParameters.FromBody col
        | :? CollisionObject3D as col -> PhysicsQueryBasicParameters.FromBody col
        | _ -> PhysicsQueryBasicParameters.New()
        
    member this.Attach (q : PhysicsPointQueryParameters2D) =
        q.CollideWithAreas <- this.CollideWithAreas
        q.CollideWithBodies <- this.CollideWithBodies
        q.CollisionMask <- this.CollisionMask
        q.Exclude <- this.Exclude |> Collections.Array<Rid>
        
    member this.Attach (q : PhysicsRayQueryParameters2D) =
        q.CollideWithAreas <- this.CollideWithAreas
        q.CollideWithBodies <- this.CollideWithBodies
        q.CollisionMask <- this.CollisionMask
        q.Exclude <- this.Exclude |> Collections.Array<Rid>
        
    member this.Attach (q : PhysicsShapeQueryParameters2D) =
        q.CollideWithAreas <- this.CollideWithAreas
        q.CollideWithBodies <- this.CollideWithBodies
        q.CollisionMask <- this.CollisionMask
        q.Exclude <- this.Exclude |> Collections.Array<Rid>
        
    member this.Attach (q : PhysicsPointQueryParameters3D) =
        q.CollideWithAreas <- this.CollideWithAreas
        q.CollideWithBodies <- this.CollideWithBodies
        q.CollisionMask <- this.CollisionMask
        q.Exclude <- this.Exclude |> Collections.Array<Rid>
        
    member this.Attach (q : PhysicsRayQueryParameters3D) =
        q.CollideWithAreas <- this.CollideWithAreas
        q.CollideWithBodies <- this.CollideWithBodies
        q.CollisionMask <- this.CollisionMask
        q.Exclude <- this.Exclude |> Collections.Array<Rid>
        
    member this.Attach (q : PhysicsShapeQueryParameters3D) =
        q.CollideWithAreas <- this.CollideWithAreas
        q.CollideWithBodies <- this.CollideWithBodies
        q.CollisionMask <- this.CollisionMask
        q.Exclude <- this.Exclude |> Collections.Array<Rid>

type IPhysicsQuery =
    abstract member Param : PhysicsQueryBasicParameters with get, set

module PhysicsQuery =
    
    let isCollideWithAreas (q : IPhysicsQuery) =
        q.Param.CollideWithAreas
        
    let setCollideWithAreas (value : bool) (q : IPhysicsQuery) =
        let param = q.Param
        q.Param <- { param with CollideWithAreas = value }
        
    let isCollideWithBodies (q : IPhysicsQuery) =
        q.Param.CollideWithBodies

    let setCollideWithBodies (value : bool) (q : IPhysicsQuery) =
        let param = q.Param
        q.Param <- { param with CollideWithBodies = value }
    
    let getCollisionMask (q : IPhysicsQuery) =
        q.Param.CollisionMask
        
    let setCollisionMask (value : uint32) (q : IPhysicsQuery) =
        let param = q.Param
        q.Param <- { param with CollisionMask = value }
    
    let getCollisionMaskValue (layer : int) (q : IPhysicsQuery) =
        let mask = getCollisionMask q
        mask |> BitFlag.getValue layer
    
    let setCollisionMaskValue (value : bool) (layer : int) (q : IPhysicsQuery) =
        let mask = getCollisionMask q
        let newMask = mask |> BitFlag.setValue layer value
        q |> setCollisionMask newMask
    
    let getExclude (q : IPhysicsQuery) =
        q.Param.Exclude
    
    let setExclude (exclude : Rid list) (q : IPhysicsQuery) =
        let param = q.Param
        q.Param <- { param with Exclude = exclude }
    
    let addExclude (exclude : Rid) (q : IPhysicsQuery) =
        q |> setExclude (exclude :: q.Param.Exclude)
        
    let removeExclude (exclude : Rid) (q : IPhysicsQuery) =
        let remain = q.Param.Exclude |> List.filter ((<>) exclude )
        q |> setExclude remain
        
    let clearExclude (q : IPhysicsQuery) =
        q |> setExclude []