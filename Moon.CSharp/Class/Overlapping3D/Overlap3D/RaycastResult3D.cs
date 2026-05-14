using Godot;
using Moon.Utils;

namespace Moon.Class;

public record struct RaycastResult3D<T> where T : GodotObject
{
    public T Collider { get ;set; }
    public Rid Rid { get; set; }
    public Vector3 Position { get; set; }
    public Vector3 Normal { get; set; }
    
    public bool HasData(string tag)
        => Collider.HasData(tag);

    public TT GetData<[MustBeVariant] TT>(string tag, TT defaultValue = default)
        => Collider.GetData(Rid, tag, defaultValue);
}