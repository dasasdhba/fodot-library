module Moon.ModuleExtensions

open System.Runtime.CompilerServices

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
