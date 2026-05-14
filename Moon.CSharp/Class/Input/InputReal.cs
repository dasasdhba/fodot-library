using Godot;

namespace Moon.Class;

[GlobalClass]
public partial class InputReal : Inputer
{
    [ExportCategory("InputReal")]
    [Export]
    public bool Disabled { get ;set; } = false;

    public override InputKey GetKey(string key)
        => Disabled ? new(false, false, false) : 
            new(Input.IsActionPressed(key), Input.IsActionJustPressed(key), Input.IsActionJustReleased(key));
}