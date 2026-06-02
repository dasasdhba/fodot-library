namespace FSharp.Extend

type ReLazy<'a>(f : (unit -> 'a) option) =
    let mutable func : (unit -> 'a) option = f
    let mutable reset = false
    let mutable value : 'a option = None
    
    new(f : unit -> 'a) =
        ReLazy(Some f)
        
    new() =
        ReLazy(None)
    
    member this.Build f =
        func <- Some f
    
    member this.Value
        with get () =
            if reset || value.IsNone then
                value <- func.Value() |> Some
                reset <- false
            value.Value
            
    member this.Rebuild() =
        reset <- true
        
    member this.Assign v =
        reset <- false
        value <- Some v
