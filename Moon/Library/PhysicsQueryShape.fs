namespace Moon.Library

open Fodot.Module
open Godot

module PhysicsQueryShape2D =
    
    let getShapes (col : CollisionObject2D) =
        col.GetShapeOwners()
        |> Array.map uint
        |> Array.filter (fun i -> col.IsShapeOwnerDisabled i |> not)
        |> Array.map (fun i ->
            seq {
                for j in 0 .. col.ShapeOwnerGetShapeCount(i) - 1 do
                    let shape = col.ShapeOwnerGetShape(i, j)
                    let gt = col.ShapeOwnerGetTransform(i)
                    yield (shape, gt)
            }
        )
        |> Seq.concat
        
module PhysicsQueryShape3D =
    
    let getShapes (col : CollisionObject3D) =
        col.GetShapeOwners()
        |> Array.map uint
        |> Array.filter (fun i -> col.IsShapeOwnerDisabled i |> not)
        |> Array.map (fun i ->
            seq {
                for j in 0 .. col.ShapeOwnerGetShapeCount(i) - 1 do
                    let shape = col.ShapeOwnerGetShape(i, j)
                    let gt = col.ShapeOwnerGetTransform(i)
                    yield (shape, gt)
            }
        )
        |> Seq.concat

type PhysicsQueryShape2D(node : CollisionObject2D, param : PhysicsQueryBasicParameters) =
    
    let state = PhysicsState2D.Get node
    
    new (node : CollisionObject2D) = PhysicsQueryShape2D(node, PhysicsQueryBasicParameters.FromBody node)
    
    interface IPhysicsQuery with
        member val Param = param with get, set
    
    member val private Col = node
    member val State = state
    member val Margin = 0f with get, set

type PhysicsShapeQuerier2D(parent : PhysicsQueryShape2D, shapes: (Shape2D * Transform2D) array) =
    
    let state = parent.State
    let param = (parent :> IPhysicsQuery).Param
    
    member this.Query (?offset : Vector2, ?maxResult : int) =
        let offset = defaultArg offset Vector2.Zero
        let maxResult = defaultArg maxResult 32
        
        use query = new PhysicsShapeQueryParameters2D()
        query |> param.Attach
        query.Margin <- parent.Margin
        
        query
        |> Seq.unfold (fun q ->
            state.SpaceState
            |> Option.bind (fun dss ->
                shapes
                |> Array.tryPick (fun (s, gt) ->
                    q.Shape <- s
                    q.Transform <- gt |> Transform2D.withOrigin (gt.Origin + offset)
                    dss.GetRestInfo q
                    |> Option.ofObj
                    |> Option.map (fun r ->
                        let res = PhysicsQueryShapeResult2D.From r
                        q.Exclude <-
                            let ex = q.Exclude
                            ex.Add res.Rid
                            ex
                        res, q
                    )
                )
            )
        )
        |> Seq.truncate maxResult
    
    member this.Cast (motion : Vector2, ?offset : Vector2) =
        let offset = defaultArg offset Vector2.Zero
        
        use query = new PhysicsShapeQueryParameters2D()
        query |> param.Attach
        query.Margin <- parent.Margin
        query.Motion <- motion
        
        state.SpaceState
        |> Option.map (fun dss ->
            shapes
            |> Array.map (fun (s, gt) ->
                query.Shape <- s
                query.Transform <- gt |> Transform2D.withOrigin (gt.Origin + offset)
                dss.CastMotion query |> PhysicsQueryMotionCastResult.From
            )
            |> Array.minBy _.SafeFraction
        )
        |> Option.defaultValue PhysicsQueryMotionCastResult.Default
        
    member this.CastAndQuery (motion : Vector2, ?offset : Vector2, ?maxResult : int) =
        let offset = defaultArg offset Vector2.Zero
        let maxResult = defaultArg maxResult 32
        
        use query = new PhysicsShapeQueryParameters2D()
        query |> param.Attach
        query.Margin <- parent.Margin
        
        query
        |> Seq.unfold (fun q ->
            state.SpaceState
            |> Option.bind (fun dss ->
                let s, gt, cast =
                    shapes
                    |> Array.map (fun (s, gt) ->
                        let gt = gt |> Transform2D.withOrigin (gt.Origin + offset)
                        q.Shape <- s
                        q.Transform <- gt
                        s, gt, dss.CastMotion q |> PhysicsQueryMotionCastResult.From
                    )
                    |> Array.minBy (fun (_, _, r) -> r.SafeFraction)
                
                let travel = min 1f (cast.UnsafeFraction + 1e-5f)
                let gt = gt |> Transform2D.withOrigin (gt.Origin + motion * travel)
                q.Shape <- s
                q.Transform <- gt
                q.Motion <- Vector2.Zero
               
                dss.GetRestInfo q
                |> Option.ofObj
                |> Option.map (fun r ->
                    let res = PhysicsQueryShapeResult2D.From r
                    q.Exclude <-
                        let ex = q.Exclude
                        ex.Add res.Rid
                        ex
                    (cast, res), q
                )
            )
        )
        |> Seq.truncate maxResult
        
type PhysicsQueryShape2D with

    member this.BuildBy (shapes : (Shape2D * Transform2D) array) =
        PhysicsShapeQuerier2D(this, shapes)
        
    member this.Build ()=
        this.BuildBy (PhysicsQueryShape2D.getShapes this.Col |> Array.ofSeq)
        
type PhysicsQueryShape3D(node : CollisionObject3D, param : PhysicsQueryBasicParameters) =
    
    let state = PhysicsState3D.Get node
    
    new (node : CollisionObject3D) = PhysicsQueryShape3D(node, PhysicsQueryBasicParameters.FromBody node)
    
    interface IPhysicsQuery with
        member val Param = param with get, set
    
    member val private Col = node
    member val State = state
    member val Margin = 0f with get, set

type PhysicsShapeQuerier3D(parent : PhysicsQueryShape3D, shapes: (Shape3D * Transform3D) array) =
    
    let state = parent.State
    let param = (parent :> IPhysicsQuery).Param
    
    member this.Query (?offset : Vector3, ?maxResult : int) =
        let offset = defaultArg offset Vector3.Zero
        let maxResult = defaultArg maxResult 32
        
        use query = new PhysicsShapeQueryParameters3D()
        query |> param.Attach
        query.Margin <- parent.Margin
        
        query
        |> Seq.unfold (fun q ->
            state.SpaceState
            |> Option.bind (fun dss ->
                shapes
                |> Array.tryPick (fun (s, gt) ->
                    q.Shape <- s
                    q.Transform <- gt |> Transform3D.withOrigin (gt.Origin + offset)
                    dss.GetRestInfo q
                    |> Option.ofObj
                    |> Option.map (fun r ->
                        let res = PhysicsQueryShapeResult3D.From r
                        q.Exclude <-
                            let ex = q.Exclude
                            ex.Add res.Rid
                            ex
                        res, q
                    )
                )
            )
        )
        |> Seq.truncate maxResult
    
    member this.Cast (motion : Vector3, ?offset : Vector3) =
        let offset = defaultArg offset Vector3.Zero
        
        use query = new PhysicsShapeQueryParameters3D()
        query |> param.Attach
        query.Margin <- parent.Margin
        query.Motion <- motion
        
        state.SpaceState
        |> Option.map (fun dss ->
            shapes
            |> Array.map (fun (s, gt) ->
                query.Shape <- s
                query.Transform <- gt |> Transform3D.withOrigin (gt.Origin + offset)
                dss.CastMotion query |> PhysicsQueryMotionCastResult.From
            )
            |> Array.minBy _.SafeFraction
        )
        |> Option.defaultValue PhysicsQueryMotionCastResult.Default
    
    member this.CastAndQuery (motion : Vector3, ?offset : Vector3, ?maxResult : int) =
        let offset = defaultArg offset Vector3.Zero
        let maxResult = defaultArg maxResult 32
        
        use query = new PhysicsShapeQueryParameters3D()
        query |> param.Attach
        query.Margin <- parent.Margin
        
        query
        |> Seq.unfold (fun q ->
            state.SpaceState
            |> Option.bind (fun dss ->
                let s, gt, cast =
                    shapes
                    |> Array.map (fun (s, gt) ->
                        let gt = gt |> Transform3D.withOrigin (gt.Origin + offset)
                        q.Shape <- s
                        q.Transform <- gt
                        s, gt, dss.CastMotion q |> PhysicsQueryMotionCastResult.From
                    )
                    |> Array.minBy (fun (_, _, r) -> r.SafeFraction)
                
                let travel = min 1f (cast.UnsafeFraction + 1e-5f)
                let gt = gt |> Transform3D.withOrigin (gt.Origin + motion * travel)
                q.Shape <- s
                q.Transform <- gt
                q.Motion <- Vector3.Zero
               
                dss.GetRestInfo q
                |> Option.ofObj
                |> Option.map (fun r ->
                    let res = PhysicsQueryShapeResult3D.From r
                    q.Exclude <-
                        let ex = q.Exclude
                        ex.Add res.Rid
                        ex
                    (cast, res), q
                )
            )
        )
        |> Seq.truncate maxResult
    
type PhysicsQueryShape3D with

    member this.BuildBy (shapes : (Shape3D * Transform3D) array) =
        PhysicsShapeQuerier3D(this, shapes)
        
    member this.Build ()=
        this.BuildBy (PhysicsQueryShape3D.getShapes this.Col |> Array.ofSeq)