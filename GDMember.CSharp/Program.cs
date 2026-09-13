using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

internal static class Program
{
    private sealed record Member(
        string Name,
        string ReturnType,
        string Parameters,
        string Arguments,
        string DelegateType,
        bool IsVoid,
        string File,
        int Line);

    private sealed class PartialClass
    {
        public required string Namespace { get; init; }
        public required IReadOnlyList<string> ContainingTypes { get; init; }
        public required string Name { get; init; }
        public required string Accessibility { get; init; }
        public HashSet<string> Usings { get; } = new(StringComparer.Ordinal);
        public List<Member> Members { get; } = [];
    }

    private static int Main(string[] args)
    {
        try
        {
            var output = GetOption(args, "--output");
            var inputList = GetOption(args, "--input-list");

            Directory.CreateDirectory(output);
            foreach (var oldFile in Directory.EnumerateFiles(output, "*.g.cs"))
                File.Delete(oldFile);

            var classes = new Dictionary<string, PartialClass>(StringComparer.Ordinal);
            foreach (var file in File.ReadLines(inputList).Where(line => !string.IsNullOrWhiteSpace(line)))
            {
                if (!file.EndsWith(".cs", StringComparison.OrdinalIgnoreCase) || file.EndsWith(".g.cs", StringComparison.OrdinalIgnoreCase))
                    continue;

                ScanFile(Path.GetFullPath(file), classes);
            }

            foreach (var target in classes.Values.Where(c => c.Members.Count > 0))
            {
                var path = Path.Combine(output, GetFileName(target));
                File.WriteAllText(path, Generate(target), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            }

            return 0;
        }
        catch (Exception error)
        {
            Console.Error.WriteLine($"GDMember.CSharp: {error.Message}");
            return 1;
        }
    }

    private static void ScanFile(string file, Dictionary<string, PartialClass> classes)
    {
        var source = File.ReadAllText(file);
        var tree = CSharpSyntaxTree.ParseText(source, path: file);
        var root = tree.GetCompilationUnitRoot();

        foreach (var classDeclaration in root.DescendantNodes().OfType<ClassDeclarationSyntax>())
        {
            var members = classDeclaration.Members
                .OfType<MethodDeclarationSyntax>()
                .Where(HasGDMemberAttribute)
                .Select(method => ParseMember(method, file))
                .ToList();

            if (members.Count == 0)
                continue;

            if (!classDeclaration.Modifiers.Any(SyntaxKind.PartialKeyword))
                throw new InvalidOperationException($"{file}: class '{classDeclaration.Identifier}' must be partial to use GDMember.");

            var ns = string.Join(".", classDeclaration.Ancestors()
                .OfType<BaseNamespaceDeclarationSyntax>()
                .Reverse()
                .Select(namespaceDeclaration => namespaceDeclaration.Name.ToString()));
            var containingTypes = classDeclaration.Ancestors()
                .OfType<TypeDeclarationSyntax>()
                .Reverse()
                .Select(type => type.Identifier.Text)
                .ToArray();
            var key = $"{ns}|{string.Join('.', containingTypes)}|{classDeclaration.Identifier.Text}";

            if (!classes.TryGetValue(key, out var target))
            {
                target = new PartialClass
                {
                    Namespace = ns,
                    ContainingTypes = containingTypes,
                    Name = classDeclaration.Identifier.Text,
                    Accessibility = GetAccessibility(classDeclaration.Modifiers)
                };
                classes.Add(key, target);
            }

            foreach (var usingDirective in root.DescendantNodes().OfType<UsingDirectiveSyntax>())
                target.Usings.Add(usingDirective.ToString());

            foreach (var member in members)
            {
                if (target.Members.Any(existing => existing.Name == member.Name))
                    throw new InvalidOperationException($"{file}: overloaded GDMember '{member.Name}' is not supported.");

                target.Members.Add(member);
            }
        }
    }

    private static Member ParseMember(MethodDeclarationSyntax method, string file)
    {
        var line = method.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
        if (!method.Modifiers.Any(SyntaxKind.PartialKeyword))
            throw new InvalidOperationException($"{file}:{line}: GDMember method '{method.Identifier}' must be partial.");
        if (method.Body is not null || method.ExpressionBody is not null)
            throw new InvalidOperationException($"{file}:{line}: GDMember method '{method.Identifier}' must be a declaration without a body.");
        if (method.Modifiers.Any(SyntaxKind.StaticKeyword))
            throw new InvalidOperationException($"{file}:{line}: static GDMember method '{method.Identifier}' is not supported.");
        if (method.TypeParameterList is not null)
            throw new InvalidOperationException($"{file}:{line}: generic GDMember method '{method.Identifier}' is not supported.");

        foreach (var parameter in method.ParameterList.Parameters)
        {
            if (parameter.Type is null || parameter.Default is not null || parameter.Modifiers.Count > 0)
                throw new InvalidOperationException($"{file}:{line}: GDMember method '{method.Identifier}' only supports ordinary typed parameters without defaults.");
        }

        var parameterTypes = method.ParameterList.Parameters.Select(parameter => parameter.Type!.ToString()).ToArray();
        var isVoid = method.ReturnType.ToString() == "void";
        if (parameterTypes.Length > 16)
            throw new InvalidOperationException($"{file}:{line}: GDMember method '{method.Identifier}' supports at most 16 parameters.");

        var delegateType = isVoid
            ? parameterTypes.Length == 0 ? "System.Action" : $"System.Action<{string.Join(", ", parameterTypes)}>"
            : parameterTypes.Length == 0
                ? $"System.Func<{method.ReturnType}>"
                : $"System.Func<{string.Join(", ", parameterTypes)}, {method.ReturnType}>";

        return new Member(
            method.Identifier.Text,
            method.ReturnType.ToString(),
            method.ParameterList.ToString(),
            string.Join(", ", method.ParameterList.Parameters.Select(parameter => parameter.Identifier.Text)),
            delegateType,
            isVoid,
            file,
            line);
    }

    private static bool HasGDMemberAttribute(MethodDeclarationSyntax method) =>
        method.AttributeLists
            .SelectMany(attributes => attributes.Attributes)
            .Any(attribute =>
            {
                var name = attribute.Name.ToString();
                return name is "GDMember" or "GDMemberAttribute" || name.EndsWith(".GDMember", StringComparison.Ordinal) || name.EndsWith(".GDMemberAttribute", StringComparison.Ordinal);
            });

    private static string Generate(PartialClass target)
    {
        var builder = new StringBuilder();
        builder.AppendLine("// <auto-generated by GDMember.CSharp />");
        builder.AppendLine("#nullable enable");
        builder.AppendLine();

        foreach (var usingDirective in target.Usings.Order(StringComparer.Ordinal))
            builder.AppendLine(usingDirective);
        if (target.Usings.Count > 0)
            builder.AppendLine();

        if (string.IsNullOrWhiteSpace(target.Namespace))
            GenerateClass(builder, target, 0);
        else
        {
            builder.AppendLine($"namespace {target.Namespace};");
            builder.AppendLine();
            GenerateClass(builder, target, 0);
        }

        return builder.ToString();
    }

    private static void GenerateClass(StringBuilder builder, PartialClass target, int indent)
    {
        var prefix = new string(' ', indent * 4);
        builder.AppendLine($"{prefix}{target.Accessibility} partial class {target.Name}");
        builder.AppendLine($"{prefix}{{");

        foreach (var member in target.Members)
        {
            var fieldName = $"_FSharpInvoke{member.Name}";
            builder.AppendLine($"{prefix}    public {member.DelegateType}? {fieldName};");
            var modifiers = member.IsVoid ? "" : "";
            builder.AppendLine($"{prefix}    {modifiers}public partial {member.ReturnType} {member.Name}{member.Parameters}");
            if (member.IsVoid)
                builder.AppendLine($"{prefix}        => {fieldName}?.Invoke({member.Arguments});");
            else
                builder.AppendLine($"{prefix}        => ({fieldName} ?? throw new System.InvalidOperationException(\"F# GDMember '{member.Name}' has not been initialized.\"))({member.Arguments});");
            builder.AppendLine();
        }

        builder.AppendLine($"{prefix}}}");
    }

    private static string GetAccessibility(SyntaxTokenList modifiers)
    {
        if (modifiers.Any(SyntaxKind.PublicKeyword))
            return "public";
        if (modifiers.Any(SyntaxKind.ProtectedKeyword))
            return "protected";
        if (modifiers.Any(SyntaxKind.InternalKeyword))
            return "internal";
        if (modifiers.Any(SyntaxKind.PrivateKeyword))
            return "private";
        return "internal";
    }

    private static string GetFileName(PartialClass target)
    {
        var parts = target.ContainingTypes.Append(target.Name)
            .Prepend(target.Namespace.Replace('.', '_'))
            .Where(part => !string.IsNullOrWhiteSpace(part))
            .Select(part => string.Concat(part.Select(character => char.IsLetterOrDigit(character) ? character : '_')));
        return $"{string.Join('_', parts)}.GDMember.g.cs";
    }

    private static string GetOption(string[] args, string option)
    {
        var index = Array.IndexOf(args, option);
        if (index < 0 || index + 1 >= args.Length)
            throw new ArgumentException($"Missing {option}.");
        return args[index + 1];
    }
}
