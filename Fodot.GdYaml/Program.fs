open Fodot.GdYaml.Parser

[<EntryPoint>]
let main args =
    if args.Length < 1 then
        printfn "Usage: Fodot.GdYaml <inputDir>"
        1
    else
        let inputDir = args[0]
        
        createFsBinding inputDir
        0