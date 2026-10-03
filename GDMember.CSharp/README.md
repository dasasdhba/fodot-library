# GDMember.CSharp

Roslyn incremental source generator implementing `[Moon.GDMember]` partial
methods through public `_FSharpInvokeMethodName` delegate fields.

Enable with `<EnableGDMemberCSharp>true</EnableGDMemberCSharp>` in this
repository. `Directory.Build.targets` references this project as an analyzer;
there is no command-line generation step or input-file list.

```csharp
using Moon;

public partial class Component
{
    [GDMember]
    public partial int GetValue();

    [GDMember]
    public partial void Update(float delta);
}
```

`GDMemberAttribute` remains in Moon.Bridge. Generated fields retain the existing
names and types expected by the GDMember.FSharp Myriad generator. Void methods
do nothing until their callback is assigned; value-returning methods throw
`InvalidOperationException` until assigned, matching the previous generator.
The optional attribute name argument retains its previous behavior (it does not
rename the C# delegate field).

Methods and enclosing types must be partial. Static/generic methods, annotated
overloads, ref returns, and parameters with modifiers/defaults are unsupported.
Up to 16 parameters are supported. Invalid declarations report `GDMEMBER001`
at their source location.

GDMember.FSharp and Godot.Warning.Generator retain their existing build-time
generation workflows.
