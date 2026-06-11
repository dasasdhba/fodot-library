module Moon.Library.PhysicsMotion

open FSharp.Extend
open Godot
    
type PhysicsShapeQuerier2D with

    member this.Collide (motion : Vector2, ?offset : Vector2, ?maxResult : int, ?margin : float32) =
        let overlapped =
            this.QueryInside(?offset = offset, ?maxResult = maxResult, ?margin = margin)
            |> Array.ofSeq
        
        let filterMargin (r: PhysicsQueryShapeResult2D) m =
            let shift = defaultArg offset Vector2.Zero - (motion.Normalized() * m)
            
            this.QueryInside(shift, ?maxResult = maxResult, ?margin = margin)
            |> Seq.tryFind (fun i -> i.Rid = r.Rid)
            |> function
                | None ->
                    let minus = - m / motion.Length()
                    let c = this.Cast(motion, shift, ?margin = margin, hitFromInside = false)
                    let c = {
                        c with
                            SafeFraction = c.SafeFraction + minus
                            UnsafeFraction = c.UnsafeFraction + minus
                    }
                    Some (c, r)
                | _ -> None
        
        this.CastAndQuery(motion, ?offset = offset, ?maxResult = maxResult, ?margin = margin, hitFromInside = true)
        |> Seq.choose (fun (c, r) ->
            match 
                r
                |> PhysicsQueryResult.getOneWayParameters2D
            with
            
            | Some (d, _) when d.Dot motion <= 0f -> None
            | Some (_, m) when
                overlapped
                |> Array.tryFind (fun o -> o.Rid = r.Rid)
                |> Option.isSome
                 -> filterMargin r m
            | _ -> Some (c, r)
        )
        
        |> Seq.tryMinBy (fst >> _.SafeFraction)
            