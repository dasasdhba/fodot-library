using Godot;
using Moon.Utils;

namespace Moon.Class;

public record struct OverlapResult3D<T> where T : GodotObject
{
    public T Collider { get ;set; }
    public Rid Rid { get; set; }
    
    public bool HasData(string tag)
       => Collider.HasData(tag);

    public TT GetData<[MustBeVariant] TT>(string tag, TT defaultValue = default)
        => Collider.GetData(Rid, tag, defaultValue);
}