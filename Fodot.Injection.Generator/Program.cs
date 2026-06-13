using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Fodot.Injection.Generator;

internal static class Program
{
    public static int Main(string[] args)
    {
        if (!TryParseArgs(args, out string outputDirectory, out ImmutableArray<string> inputFiles, out string error))
        {
            Console.Error.WriteLine(error);
            return 1;
        }

        Directory.CreateDirectory(outputDirectory);

        foreach (string oldFile in Directory.EnumerateFiles(outputDirectory, "*.ChildOf.g.cs"))
            File.Delete(oldFile);

        string oldAttributeFile = Path.Combine(outputDirectory, "ChildOfAttribute.g.cs");
        if (File.Exists(oldAttributeFile))
            File.Delete(oldAttributeFile);

        string oldAttributeUidFile = Path.Combine(outputDirectory, "ChildOfAttribute.g.cs.uid");
        if (File.Exists(oldAttributeUidFile))
            File.Delete(oldAttributeUidFile);

        foreach (ChildOfTarget target in ReadTargets(inputFiles))
        {
            string path = Path.Combine(outputDirectory, target.HintName);
            File.WriteAllText(path, GenerateSource(target), Encoding.UTF8);
        }

        return 0;
    }

    private static bool TryParseArgs(
        string[] args,
        out string outputDirectory,
        out ImmutableArray<string> inputFiles,
        out string error)
    {
        outputDirectory = string.Empty;
        error = string.Empty;
        var files = ImmutableArray.CreateBuilder<string>();
        var inputLists = ImmutableArray.CreateBuilder<string>();

        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "--output")
            {
                if (i + 1 >= args.Length)
                {
                    error = "Missing value for --output.";
                    inputFiles = [];
                    return false;
                }

                outputDirectory = args[++i];
                continue;
            }

            if (args[i] == "--input-list")
            {
                if (i + 1 >= args.Length)
                {
                    error = "Missing value for --input-list.";
                    inputFiles = [];
                    return false;
                }

                inputLists.Add(args[++i]);
                continue;
            }

            files.Add(args[i]);
        }

        if (string.IsNullOrWhiteSpace(outputDirectory))
        {
            error = "Missing --output.";
            inputFiles = [];
            return false;
        }

        foreach (string inputList in inputLists.Where(File.Exists))
        {
            foreach (string file in File.ReadLines(inputList, Encoding.UTF8))
            {
                if (!string.IsNullOrWhiteSpace(file))
                    files.Add(file.Trim());
            }
        }

        inputFiles = files
            .Where(File.Exists)
            .Where(static file => !file.EndsWith(".g.cs", StringComparison.OrdinalIgnoreCase))
            .ToImmutableArray();

        return true;
    }

    private static IEnumerable<ChildOfTarget> ReadTargets(ImmutableArray<string> inputFiles)
    {
        foreach (string file in inputFiles)
        {
            string text = File.ReadAllText(file, Encoding.UTF8);
            CompilationUnitSyntax root = CSharpSyntaxTree.ParseText(text, path: file).GetCompilationUnitRoot();

            foreach (ClassDeclarationSyntax classDeclaration in root.DescendantNodes().OfType<ClassDeclarationSyntax>())
            {
                string? parentType = GetChildOfParentType(classDeclaration);
                if (string.IsNullOrWhiteSpace(parentType))
                    continue;

                string? namespaceName = classDeclaration.Ancestors()
                    .OfType<BaseNamespaceDeclarationSyntax>()
                    .FirstOrDefault()
                    ?.Name
                    .ToString();

                ImmutableArray<ClassShell> containingTypes = classDeclaration
                    .Ancestors()
                    .OfType<ClassDeclarationSyntax>()
                    .Reverse()
                    .Select(static type => new ClassShell(type.Identifier.Text, GetAccessibility(type)))
                    .ToImmutableArray();

                string accessibility = GetAccessibility(classDeclaration);
                string hintName = BuildHintName(namespaceName, containingTypes, classDeclaration.Identifier.Text);

                yield return new ChildOfTarget(
                    classDeclaration.Identifier.Text,
                    accessibility,
                    namespaceName ?? string.Empty,
                    containingTypes,
                    parentType.Trim(),
                    hintName);
            }
        }
    }

    private static string? GetChildOfParentType(ClassDeclarationSyntax classDeclaration)
    {
        foreach (AttributeSyntax attribute in classDeclaration.AttributeLists.SelectMany(static list => list.Attributes))
        {
            string name = attribute.Name.ToString();
            if (name is not "ChildOf" and not "ChildOfAttribute" &&
                !name.EndsWith(".ChildOf", StringComparison.Ordinal) &&
                !name.EndsWith(".ChildOfAttribute", StringComparison.Ordinal))
                continue;

            AttributeArgumentSyntax? firstArgument = attribute.ArgumentList?.Arguments.FirstOrDefault();
            if (firstArgument?.Expression is LiteralExpressionSyntax literal &&
                literal.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.StringLiteralExpression))
                return literal.Token.ValueText;
        }

        return null;
    }

    private static string GetAccessibility(ClassDeclarationSyntax classDeclaration)
    {
        foreach (SyntaxToken modifier in classDeclaration.Modifiers)
        {
            if (modifier.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.PublicKeyword) ||
                modifier.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.InternalKeyword) ||
                modifier.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.PrivateKeyword) ||
                modifier.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.ProtectedKeyword))
                return modifier.Text;
        }

        return string.Empty;
    }

    private static string BuildHintName(
        string? namespaceName,
        ImmutableArray<ClassShell> containingTypes,
        string typeName)
    {
        string fullName = string.Join("_", new[]
            {
                namespaceName,
                string.Join("_", containingTypes.Select(static type => type.TypeName)),
                typeName
            }
            .Where(static part => !string.IsNullOrWhiteSpace(part)));

        foreach (char invalid in Path.GetInvalidFileNameChars())
            fullName = fullName.Replace(invalid, '_');

        return fullName.Replace('.', '_') + ".ChildOf.g.cs";
    }

    private static string GenerateSource(ChildOfTarget target)
    {
        var source = new StringBuilder();
        source.AppendLine("// <auto-generated />");
        source.AppendLine("#nullable enable");
        source.AppendLine("#if DEBUG");
        source.AppendLine("using Godot;");
        source.AppendLine();

        if (!string.IsNullOrEmpty(target.Namespace))
        {
            source.Append("namespace ").Append(target.Namespace).AppendLine(";");
            source.AppendLine();
        }

        foreach (ClassShell containingType in target.ContainingTypes)
        {
            AppendAccessibility(source, containingType.Accessibility);
            source.Append("partial class ").Append(containingType.TypeName).AppendLine();
            source.AppendLine("{");
        }

        string indent = new(' ', target.ContainingTypes.Length * 4);
        source.Append(indent).AppendLine("[Tool]");
        source.Append(indent);
        AppendAccessibility(source, target.Accessibility);
        source.Append("partial class ").Append(target.TypeName).AppendLine(" : ISerializationListener");
        source.Append(indent).AppendLine("{");
        source.Append(indent).AppendLine("    public void OnBeforeSerialize()");
        source.Append(indent).AppendLine("    {");
        source.Append(indent).AppendLine("        TreeEntered -= UpdateConfigurationWarnings;");
        source.Append(indent).AppendLine("    }");
        source.AppendLine();
        source.Append(indent).AppendLine("    public void OnAfterDeserialize()");
        source.Append(indent).AppendLine("    {");
        //source.Append(indent).AppendLine("        TreeEntered += UpdateConfigurationWarnings;");
        source.Append(indent).AppendLine("    }");
        source.AppendLine();
        source.Append(indent).Append("    public ").Append(target.TypeName).AppendLine("() : base()");
        source.Append(indent).AppendLine("    {");
        source.Append(indent).AppendLine("        if (!Engine.IsEditorHint()) return;");
        source.AppendLine();
        source.Append(indent).AppendLine("        TreeEntered += UpdateConfigurationWarnings;");
        source.Append(indent).AppendLine("    }");
        source.AppendLine();
        source.Append(indent).AppendLine("    public override string[] _GetConfigurationWarnings()");
        source.Append(indent).AppendLine("    {");
        source.Append(indent).Append("        if (GetParentOrNull<").Append(target.ParentType).AppendLine(">() == null)");
        source.Append(indent).Append("            return [\"").Append(EscapeString(target.TypeName)).Append(" must be a child of a ").Append(EscapeString(target.ParentType)).AppendLine("\"];");
        source.Append(indent).AppendLine("        return [];");
        source.Append(indent).AppendLine("    }");
        source.Append(indent).AppendLine("}");

        for (int i = 0; i < target.ContainingTypes.Length; i++)
            source.AppendLine("}");

        source.AppendLine("#endif");
        return source.ToString();
    }

    private static void AppendAccessibility(StringBuilder source, string accessibility)
    {
        if (!string.IsNullOrWhiteSpace(accessibility))
            source.Append(accessibility).Append(' ');
    }

    private static string EscapeString(string value)
        => value.Replace("\\", "\\\\").Replace("\"", "\\\"");

    private sealed record ClassShell(string TypeName, string Accessibility);

    private sealed record ChildOfTarget(
        string TypeName,
        string Accessibility,
        string Namespace,
        ImmutableArray<ClassShell> ContainingTypes,
        string ParentType,
        string HintName);
}
