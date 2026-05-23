module Fodot.Module.BitFlag

let getValue (layer : int) (flag : uint) =
    ((flag >>> (layer - 1)) &&& 1u) = 1u
    
let setValue (layer : int) (value : bool) (flag : uint) =
    let n = layer - 1
    if value then
        flag ||| (1u <<< n)
    else
        flag &&& ~~~(1u <<< n)