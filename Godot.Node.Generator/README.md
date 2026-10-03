# Godot.Node.Generator

Roslyn incremental source generator for cached Godot node properties (C# 13+).

Enable it in a consuming C# project using the repository's `Directory.Build.targets`:

```xml
<EnableGodotNodeGenerator>true</EnableGodotNodeGenerator>
```

For use outside this repository, reference the generator as an analyzer:

```xml
<ProjectReference Include="path/to/Godot.Node.Generator.csproj"
                  OutputItemType="Analyzer" ReferenceOutputAssembly="false" />
```

```csharp
using Godot;

public partial class Player : Node2D
{
    [NodeGet] // Defaults to "%Anim".
    public partial AnimatedSprite2D Anim { get; }

    [NodeGet("Visual/Sprite")]
    public partial Sprite2D Sprite { get; }
}
```

The generator supplies `Godot.NodeGetAttribute` and a private cache field per
property. The getter uses `cache ??= GetNode<T>(path)`: lookup happens on first
access, then the reference is reused. The caller must access it only after the
node is available. Cache invalidation after freeing/replacing a node is not
automatic, matching a handwritten lazy getter.

Properties must be instance, getter-only partial declarations with non-nullable
Godot.Node-derived types. Their class and all enclosing types must be partial.
Invalid declarations report `NODEGET001`.
