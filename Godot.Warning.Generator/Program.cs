using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Godot.Warning.Generator;

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

        foreach (string oldFile in Directory.EnumerateFiles(outputDirectory, "*.g.cs"))
            File.Delete(oldFile);

        foreach (WarningTarget target in ReadTargets(inputFiles))
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

    private static IEnumerable<WarningTarget> ReadTargets(ImmutableArray<string> inputFiles)
    {
        foreach (string file in inputFiles)
        {
            string text = File.ReadAllText(file, Encoding.UTF8);
            CompilationUnitSyntax root = CSharpSyntaxTree.ParseText(text, path: file).GetCompilationUnitRoot();

            foreach (ClassDeclarationSyntax classDeclaration in root.DescendantNodes().OfType<ClassDeclarationSyntax>())
            {
                ImmutableArray<string> parentTypes = GetChildOfParentTypes(classDeclaration);
                ImmutableArray<PropertyWarning> propertyWarnings = GetPropertyWarnings(classDeclaration);

                if (parentTypes.IsEmpty && propertyWarnings.IsEmpty)
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

                yield return new WarningTarget(
                    classDeclaration.Identifier.Text,
                    accessibility,
                    namespaceName ?? string.Empty,
                    containingTypes,
                    parentTypes,
                    propertyWarnings,
                    hintName);
            }
        }
    }

    private static ImmutableArray<string> GetChildOfParentTypes(ClassDeclarationSyntax classDeclaration)
    {
        foreach (AttributeSyntax attribute in classDeclaration.AttributeLists.SelectMany(static list => list.Attributes))
        {
            if (!IsAttribute(attribute, "ChildOf"))
                continue;

            AttributeArgumentSyntax? firstArgument = attribute.ArgumentList?.Arguments.FirstOrDefault();
            if (firstArgument?.Expression is not LiteralExpressionSyntax literal ||
                !literal.IsKind(SyntaxKind.StringLiteralExpression))
                continue;

            return literal.Token.ValueText
                .Split(',')
                .Select(static parentType => parentType.Trim())
                .Where(static parentType => !string.IsNullOrEmpty(parentType))
                .Distinct(StringComparer.Ordinal)
                .ToImmutableArray();
        }

        return [];
    }

    private static ImmutableArray<PropertyWarning> GetPropertyWarnings(ClassDeclarationSyntax classDeclaration)
    {
        var warnings = ImmutableArray.CreateBuilder<PropertyWarning>();

        foreach (PropertyDeclarationSyntax property in classDeclaration.Members.OfType<PropertyDeclarationSyntax>())
        {
            AttributeSyntax? notNullString = property.AttributeLists
                .SelectMany(static list => list.Attributes)
                .FirstOrDefault(static attribute => IsAttribute(attribute, "NotNullString"));

            if (notNullString is not null)
            {
                warnings.Add(new PropertyWarning(
                    PropertyWarningKind.NotNullString,
                    GetAccessibility(property),
                    property.Type.ToString(),
                    property.Identifier.Text,
                    GetFirstArgumentExpression(notNullString)));
                continue;
            }

            AttributeSyntax? notNull = property.AttributeLists
                .SelectMany(static list => list.Attributes)
                .FirstOrDefault(static attribute => IsAttribute(attribute, "NotNull"));

            if (notNull is not null)
            {
                warnings.Add(new PropertyWarning(
                    PropertyWarningKind.NotNull,
                    GetAccessibility(property),
                    property.Type.ToString(),
                    property.Identifier.Text,
                    null));
            }
        }

        return warnings.ToImmutable();
    }

    private static string? GetFirstArgumentExpression(AttributeSyntax attribute)
        => attribute.ArgumentList?.Arguments.FirstOrDefault()?.Expression.ToString();

    private static bool IsAttribute(AttributeSyntax attribute, string shortName)
    {
        string name = attribute.Name.ToString();
        return name == shortName ||
            name == shortName + "Attribute" ||
            name.EndsWith("." + shortName, StringComparison.Ordinal) ||
            name.EndsWith("." + shortName + "Attribute", StringComparison.Ordinal);
    }

    private static string GetAccessibility(MemberDeclarationSyntax declaration)
    {
        foreach (SyntaxToken modifier in declaration.Modifiers)
        {
            if (modifier.IsKind(SyntaxKind.PublicKeyword) ||
                modifier.IsKind(SyntaxKind.InternalKeyword) ||
                modifier.IsKind(SyntaxKind.PrivateKeyword) ||
                modifier.IsKind(SyntaxKind.ProtectedKeyword))
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

        return fullName.Replace('.', '_') + ".GodotWarning.g.cs";
    }

    private static string GenerateSource(WarningTarget target)
    {
        var source = new StringBuilder();
        source.AppendLine("// <auto-generated />");
        source.AppendLine("#nullable enable");
        source.AppendLine("#pragma warning disable CS8618");
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
        if (!target.PropertyWarnings.IsEmpty)
        {
            source.Append(indent);
            AppendAccessibility(source, target.Accessibility);
            source.Append("partial class ").Append(target.TypeName).AppendLine();
            source.Append(indent).AppendLine("{");

            foreach (PropertyWarning property in target.PropertyWarnings)
                AppendProperty(source, indent, property);

            source.Append(indent).AppendLine("}");
            source.AppendLine();
        }

        source.AppendLine("#if DEBUG");
        source.Append(indent).AppendLine("[Tool]");
        source.Append(indent);
        AppendAccessibility(source, target.Accessibility);
        source.Append("partial class ").Append(target.TypeName);
        if (!target.ParentTypes.IsEmpty)
            source.Append(" : ISerializationListener");
        source.AppendLine();
        source.Append(indent).AppendLine("{");

        if (!target.ParentTypes.IsEmpty)
            AppendSerializationHooks(source, indent, target.TypeName);

        AppendWarningsMethod(source, indent, target);

        source.Append(indent).AppendLine("}");
        source.AppendLine("#endif");

        for (int i = 0; i < target.ContainingTypes.Length; i++)
            source.AppendLine("}");

        return source.ToString();
    }

    private static void AppendSerializationHooks(StringBuilder source, string indent, string typeName)
    {
        source.Append(indent).AppendLine("    public void OnBeforeSerialize()");
        source.Append(indent).AppendLine("    {");
        source.Append(indent).AppendLine("        TreeEntered -= UpdateConfigurationWarnings;");
        source.Append(indent).AppendLine("    }");
        source.AppendLine();
        source.Append(indent).AppendLine("    public void OnAfterDeserialize()");
        source.Append(indent).AppendLine("    {");
        source.Append(indent).AppendLine("    }");
        source.AppendLine();
        source.Append(indent).Append("    public ").Append(typeName).AppendLine("() : base()");
        source.Append(indent).AppendLine("    {");
        source.Append(indent).AppendLine("        if (!Engine.IsEditorHint()) return;");
        source.AppendLine();
        source.Append(indent).AppendLine("        TreeEntered += UpdateConfigurationWarnings;");
        source.Append(indent).AppendLine("    }");
        source.AppendLine();
    }

    private static void AppendProperty(StringBuilder source, string indent, PropertyWarning property)
    {
        source.Append(indent).Append("    ");
        AppendAccessibility(source, property.Accessibility);
        source.Append("partial ").Append(property.TypeName).Append(' ').Append(property.Name).AppendLine();
        source.Append(indent).AppendLine("    {");
        source.Append(indent).Append("        get => _").Append(property.Name).AppendLine(";");
        source.Append(indent).AppendLine("        set");
        source.Append(indent).AppendLine("        {");
        source.Append(indent).AppendLine("#if DEBUG");
        source.Append(indent).AppendLine("            UpdateConfigurationWarnings();");
        source.Append(indent).AppendLine("#endif");
        source.Append(indent).Append("            _").Append(property.Name).AppendLine(" = value;");
        source.Append(indent).AppendLine("        }");
        source.Append(indent).AppendLine("    }");
        source.Append(indent).Append("    private ").Append(property.TypeName).Append(" _").Append(property.Name);
        if (!string.IsNullOrWhiteSpace(property.Initializer))
            source.Append(" = ").Append(property.Initializer);
        source.AppendLine(";");
        source.AppendLine();
    }

    private static void AppendWarningsMethod(StringBuilder source, string indent, WarningTarget target)
    {
        source.Append(indent).AppendLine("    public override string[] _GetConfigurationWarnings()");
        source.Append(indent).AppendLine("    {");
        source.Append(indent).AppendLine("        var warnings = new System.Collections.Generic.List<string>();");
        source.Append(indent).AppendLine("        var baseWarnings = base._GetConfigurationWarnings();");
        source.Append(indent).AppendLine("        if (baseWarnings is not null)");
        source.Append(indent).AppendLine("            warnings.AddRange(baseWarnings);");
        source.AppendLine();

        if (!target.ParentTypes.IsEmpty)
        {
            source.Append(indent).AppendLine("        var p = GetParentOrNull<Node>();");
            source.Append(indent).Append("        if (!(");

            for (int i = 0; i < target.ParentTypes.Length; i++)
            {
                if (i > 0)
                    source.Append(" || ");

                source.Append("p is ").Append(target.ParentTypes[i]);
            }

            source.AppendLine("))");
            source.Append(indent).Append("            warnings.Add(\"").Append(EscapeString(target.TypeName)).Append(" must be a child of a ").Append(EscapeString(string.Join(" or ", target.ParentTypes))).AppendLine("\");");
            source.AppendLine();
        }

        foreach (PropertyWarning property in target.PropertyWarnings)
        {
            if (property.Kind == PropertyWarningKind.NotNullString)
            {
                source.Append(indent).Append("        if (System.String.IsNullOrWhiteSpace(").Append(property.Name).AppendLine("))");
                source.Append(indent).Append("            warnings.Add(\"").Append(EscapeString(property.Name)).AppendLine(" should not be empty\");");
            }
            else
            {
                source.Append(indent).Append("        if (").Append(property.Name).AppendLine(" == null)");
                source.Append(indent).Append("            warnings.Add(\"").Append(EscapeString(property.Name)).AppendLine(" should not be null\");");
            }

            source.AppendLine();
        }

        source.Append(indent).AppendLine("        return warnings.ToArray();");
        source.Append(indent).AppendLine("    }");
    }

    private static void AppendAccessibility(StringBuilder source, string accessibility)
    {
        if (!string.IsNullOrWhiteSpace(accessibility))
            source.Append(accessibility).Append(' ');
    }

    private static string EscapeString(string value)
        => value.Replace("\\", "\\\\").Replace("\"", "\\\"");

    private sealed record ClassShell(string TypeName, string Accessibility);

    private sealed record WarningTarget(
        string TypeName,
        string Accessibility,
        string Namespace,
        ImmutableArray<ClassShell> ContainingTypes,
        ImmutableArray<string> ParentTypes,
        ImmutableArray<PropertyWarning> PropertyWarnings,
        string HintName);

    private sealed record PropertyWarning(
        PropertyWarningKind Kind,
        string Accessibility,
        string TypeName,
        string Name,
        string? Initializer);

    private enum PropertyWarningKind
    {
        NotNull,
        NotNullString
    }
}
