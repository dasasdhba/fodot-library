namespace GDMember.FSharp

open System
open System.IO
open System.Text
open System.Text.RegularExpressions
open Myriad.Core

module private SourceGenerator =
    type Member =
        { Name: string
          Arguments: string list }

    let private typePattern =
        Regex(
            "(?ms)^\\s*\\[<FScript\\((?<tag>.*?)\\)>\\]\\s*\\r?\\n\\s*type\\s+(?<private>private\\s+)?(?<name>[A-Za-z_][A-Za-z0-9_]*)\\s*\\(\\s*(?<node>[A-Za-z_][A-Za-z0-9_]*)\\s*:\\s*(?<nodeType>[^)]+?)\\s*\\)\\s*=\\s*(?<body>.*?)(?=^\\s*\\[<FScript\\(|\\z)",
            RegexOptions.Compiled
        )

    let private memberPattern =
        Regex(
            "(?ms)^\\s*\\[<GDMember(?:\\s*\\([^)]*\\))?\\s*>\\]\\s*\\r?\\n\\s*member\\s+this\\.(?<name>[A-Za-z_][A-Za-z0-9_]*)\\s*\\((?<args>[^)]*)\\)\\s*(?::\\s*[^=\\r\\n]+)?=\\s*(?<body>.*?)(?=^\\s*\\[<GDMember|\\z)",
            RegexOptions.Compiled
        )

    let private namespacePattern =
        Regex("(?m)^\\s*namespace\\s+(?<name>[A-Za-z_][A-Za-z0-9_.]*)", RegexOptions.Compiled)

    let private modulePattern =
        Regex("(?m)^\\s*module\\s+(?<name>[A-Za-z_][A-Za-z0-9_.]*)", RegexOptions.Compiled)

    let private openPattern =
        Regex("(?m)^\\s*open\\s+[^\\r\\n]+", RegexOptions.Compiled)

    let private splitTupleArguments (arguments: string) =
        if String.IsNullOrWhiteSpace(arguments) then
            []
        else
            let parts = ResizeArray<string>()
            let current = StringBuilder()
            let mutable parentheses = 0
            let mutable brackets = 0
            let mutable braces = 0
            let mutable angles = 0

            let appendPart () =
                parts.Add(current.ToString().Trim())
                current.Clear() |> ignore

            for character in arguments do
                match character with
                | '(' -> parentheses <- parentheses + 1
                | ')' -> parentheses <- parentheses - 1
                | '[' -> brackets <- brackets + 1
                | ']' -> brackets <- brackets - 1
                | '{' -> braces <- braces + 1
                | '}' -> braces <- braces - 1
                | '<' -> angles <- angles + 1
                | '>' when angles > 0 -> angles <- angles - 1
                | ',' when parentheses = 0 && brackets = 0 && braces = 0 && angles = 0 -> appendPart ()
                | _ -> ()

                if character <> ',' || parentheses <> 0 || brackets <> 0 || braces <> 0 || angles <> 0 then
                    current.Append(character) |> ignore

            appendPart ()
            List.ofSeq parts

    let private parseArgumentName (memberName: string) (argument: string) =
        let matchResult = Regex.Match(argument.Trim(), "^(?<name>[A-Za-z_][A-Za-z0-9_']*)\\s*(?::|$)")
        if not matchResult.Success then
            invalidArg "GDMember" $"GDMember.FSharp could not parse parameter '{argument}' on member '{memberName}'."

        matchResult.Groups["name"].Value

    let private parseMembers (body: string) =
        [ for item in memberPattern.Matches(body) do
              let args = item.Groups["args"].Value.Trim()
              let memberName = item.Groups["name"].Value
              let arguments =
                  splitTupleArguments args
                  |> List.map (parseArgumentName memberName)

              if List.length arguments > 16 then
                  invalidArg "GDMember" $"GDMember.FSharp member '{memberName}' supports at most 16 parameters."

              { Name = memberName
                Arguments = arguments } ]

    let generate (source: string) =
        let builder = StringBuilder()

        let typesWithMembers =
            [ for item in typePattern.Matches(source) do
                  let members = parseMembers item.Groups["body"].Value
                  if not members.IsEmpty then
                      yield item, members ]

        let namespaceName, scriptTypePrefix =
            if typesWithMembers.IsEmpty then
                "namespace GDMember.Generated", None
            else
                let matchResult = namespacePattern.Match(source)
                if matchResult.Success then
                    let name = matchResult.Groups["name"].Value
                    $"namespace {name}", Some name
                else
                    let moduleResult = modulePattern.Match(source)
                    if moduleResult.Success then
                        let name = moduleResult.Groups["name"].Value
                        let lastDot = name.LastIndexOf('.')
                        let parent =
                            if lastDot > 0 then
                                name.Substring(0, lastDot)
                            else
                                "GDMember.Generated"
                        $"namespace {parent}", Some name
                    else
                        "namespace GDMember.Generated", None

        let opens =
            openPattern.Matches(source)
            |> Seq.cast<Match>
            |> Seq.map (fun item -> item.Value.Trim())
            |> Seq.distinct
            |> Seq.toList

        builder.AppendLine(namespaceName) |> ignore
        for openDeclaration in opens do
            builder.AppendLine(openDeclaration) |> ignore
        builder.AppendLine() |> ignore

        for item, members in typesWithMembers do
            let tag = item.Groups["tag"].Value.Trim()
            let name = item.Groups["name"].Value
            let node = item.Groups["node"].Value
            let nodeType = item.Groups["nodeType"].Value.Trim()
            let scriptType =
                match scriptTypePrefix with
                | Some prefix -> $"{prefix}.{name}"
                | None -> name

            // Use the existing field as an overload witness: it carries the exact
            // C# signature even when the F# member has no return annotation.
            let delegateFactory = $"{name}GDMemberDelegate"
            builder.AppendLine($"type private {delegateFactory} =") |> ignore
            for arity in members |> List.map (fun memberInfo -> memberInfo.Arguments.Length) |> List.distinct do
                let typeArguments = [ for i in 1 .. arity -> $"'T{i}" ]
                let arguments = [ for i in 1 .. arity -> $"arg{i}" ]
                let lambdaArguments = if arity = 0 then "()" else String.Join(" ", arguments)
                let call = $"callback {lambdaArguments}"
                for isVoid in [true; false] do
                    let returnType = if isVoid then "unit" else "'TResult"
                    let callbackType = String.Join(" -> ", (if arity = 0 then ["unit"] else typeArguments) @ [returnType])
                    let delegateArguments = if isVoid then typeArguments else typeArguments @ [returnType]
                    let delegateName = if isVoid then "System.Action" else "System.Func"
                    let delegateType =
                        if delegateArguments.IsEmpty then delegateName
                        else delegateName + "<" + String.Join(", ", delegateArguments) + ">"
                    builder.AppendLine($"    static member Create(_: {delegateType}, callback: {callbackType}) = {delegateType}(fun {lambdaArguments} -> {call})") |> ignore
            builder.AppendLine() |> ignore

            builder.AppendLine($"[<FScript({tag})>]") |> ignore
            builder.AppendLine($"type private {name}Generated({node}: {nodeType}) =") |> ignore
            builder.AppendLine($"    let script = lazy ({node} |> FScript.get<{scriptType}>)") |> ignore

            for memberInfo in members do
                let invocationArguments = String.Join(", ", memberInfo.Arguments)
                let invocation = $"script.Value.{memberInfo.Name}({invocationArguments})"
                let lambdaArguments =
                    match memberInfo.Arguments with
                    | [] -> "()"
                    | arguments -> String.Join(" ", arguments)

                builder.AppendLine($"    do {node}._FSharpInvoke{memberInfo.Name} <- {delegateFactory}.Create({node}._FSharpInvoke{memberInfo.Name}, fun {lambdaArguments} -> {invocation})") |> ignore

        builder.ToString()

[<MyriadGenerator("GDMember.FSharp")>]
type GDMemberGenerator() =
    interface IMyriadGenerator with
        member _.ValidInputExtensions = seq { ".fs" }

        member _.Generate(context: GeneratorContext) =
            let source = File.ReadAllText(context.InputFilename)
            let generated = SourceGenerator.generate source
            Output.Source($"// <auto-generated by GDMember.FSharp />\n{generated}")
