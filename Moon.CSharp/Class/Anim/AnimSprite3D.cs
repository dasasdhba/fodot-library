using Godot;

namespace Moon.Class;

[GlobalClass]
public partial class AnimSprite3D : AnimatedSprite3D
{
    [ExportCategory("AnimSprite3D")]
    [Export]
    public bool AutoPlay { get; set; } = true;

    public AnimSprite3D() : base()
    {
        TreeEntered += () =>
        {
            ProcessCallback = AnimatedSprite3DProcessCallback.Physics;
            if (AutoPlay) Play();  
        };
    }
}