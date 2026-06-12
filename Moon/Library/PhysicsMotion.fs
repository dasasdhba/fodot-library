module Moon.Library.PhysicsMotion

open FSharp.Extend
open Godot
    
type PhysicsShapeQuerier2D with

    member this.Collide (motion : Vector2, ?maxDepth: float32, ?offset : Vector2, ?maxResult : int, ?margin : float32) =
        
        // check for initial overlap
        
        let solids, platforms =
            this.QueryInside(?offset = offset, ?maxResult = maxResult, ?margin = margin)
            |> Seq.map (fun r -> r, r |> PhysicsQueryResult.getOneWayParameters2D)
            |> Array.ofSeq
            |> Array.partition (fun (_, o) -> o |> Option.isNone)
        
        let travelSolid () =
            let maxDepth = defaultArg maxDepth 4f
            if motion = Vector2.Zero || maxDepth <= 0f then None else
            
            let dir = motion.Normalized()
            let shift = defaultArg offset Vector2.Zero - (dir * maxDepth)
            
            if
                this.QueryInside(shift, ?maxResult = maxResult, ?margin = margin)
                |> Seq.isEmpty
            then
                let len = motion.Length()
                let minus = - maxDepth / motion.Length()
                this.CastAndQuery(dir * (len + maxDepth), shift, ?margin = margin, ?maxResult = maxResult, hitFromInside = false)
                |> Seq.tryHead
                |> Option.map (fun (c, r) ->
                    let c = {
                        c with
                            SafeFraction = c.SafeFraction + minus
                            UnsafeFraction = c.UnsafeFraction + minus
                    }
                    c, r
                )
            else
                None
        
        let travelPlatform () =
            platforms
            |> Seq.map (fun (r, o) ->
                let _, m = o.Value
                let dir = motion.Normalized()
                let shift = defaultArg offset Vector2.Zero - (motion.Normalized() * m)
                this.QueryInside(shift, ?maxResult = maxResult, ?margin = margin)
                |> Seq.tryFind (fun i -> i.Rid = r.Rid)
                |> function
                    | None ->
                        let len = motion.Length()
                        let minus = - m / len
                        this.CastAndQuery(dir * (len + m), shift, ?margin = margin, ?maxResult = maxResult, hitFromInside = false)
                        |> Seq.tryHead
                        |> Option.map (fun (c, r) ->
                            let c = {
                                c with
                                    SafeFraction = c.SafeFraction + minus
                                    UnsafeFraction = c.UnsafeFraction + minus
                            }
                            c, r
                        )
                    | _ -> None
            )
            |> Seq.choose id
            |> Seq.tryMinBy (fst >> _.SafeFraction)
        
        if solids |> Array.isEmpty |> not then
            travelSolid()
            |> Option.orElseWith (fun _ ->
                this.Query(?offset = offset, ?maxResult = maxResult, ?margin = margin)
                |> Seq.tryHead
                |> Option.map (fun r -> PhysicsQueryMotionResult.Zero, r)
            )

        elif platforms |> Array.isEmpty |> not then
            travelPlatform()
            
        else
            
        // now do normal casting
        
        this.CastAndQuery(motion, ?offset = offset, ?maxResult = maxResult, ?margin = margin, hitFromInside = true)
        |> Seq.choose (fun (c, r) ->
            match 
                r
                |> PhysicsQueryResult.getOneWayParameters2D
            with
            
            | Some (d, _) when d.Dot motion <= 0f -> None
            | _ -> Some (c, r)
        )
        
        |> Seq.tryHead
            
    member this.Slide (motion : Vector2, ?offset : Vector2, ?maxResult : int, ?margin : float32) =
        ()