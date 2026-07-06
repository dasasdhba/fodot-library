module FSharp.Generic.List

/// return the first removed list if the pattern is found, otherwise return None.
/// Useful when you need to know whether the remove success. Otherwise, use List.filter instead.
let remove pattern (list : 'a list) =
    list
    |> List.tryFindIndex pattern
    |> Option.map (fun i -> list |> List.removeAt i)

/// return the last removed list if the pattern is found, otherwise return None.
/// Useful when you need to know whether the remove success. Otherwise, use List.filter instead.
let removeBack pattern (list : 'a list) =
    list
    |> List.tryFindIndexBack pattern
    |> Option.map (fun i -> list |> List.removeAt i)