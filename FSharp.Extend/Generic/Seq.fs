module FSharp.Generic.Seq

let partition predicate source =
    let yes = ResizeArray<'a>()
    let no = ResizeArray<'a>()

    for x in source do
        if predicate x then
            yes.Add x
        else
            no.Add x

    yes, no

let partitionResult source =
    let yes = ResizeArray<'a>()
    let no = ResizeArray<'b>()

    for x in source do
        match x with
        | Ok x -> yes.Add x
        | Error x -> no.Add x

    yes, no

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