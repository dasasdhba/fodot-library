module FSharp.Extend.Seq

let tryMinBy projection (source : 'a seq) =
    use e = source.GetEnumerator()

    if e.MoveNext() |> not then
        None
    else
        let mutable best = e.Current
        let mutable bestKey = projection best

        while e.MoveNext() do
            let current = e.Current
            let key = projection current

            if key < bestKey then
                best <- current
                bestKey <- key

        Some best
        
let tryMaxBy projection (source : 'a seq) =
    use e = source.GetEnumerator()

    if e.MoveNext() |> not then
        None
    else
        let mutable best = e.Current
        let mutable bestKey = projection best

        while e.MoveNext() do
            let current = e.Current
            let key = projection current

            if key > bestKey then
                best <- current
                bestKey <- key

        Some best