module Fodot.CSharp.ModuleExtensions

open System.Runtime.CompilerServices
open Fodot.Module

[<Extension>]
let CreatePhysicsTween node =
    Tween.createPhysicsWith node
    
[<Extension>]
let HasShaderParam item param =
    item |> CanvasItem.hasShaderParam param

[<Extension>]
let SetShaderParam item param value =
    item |> CanvasItem.setShaderParam param value
    
[<Extension>]
let GetShaderParam<'a> item param =
    item |> CanvasItem.getShaderParamAs<'a> param