using Godot;
using Moon.Utils;

namespace Moon.Class;

/// <summary>
/// Play audio in global scope(viewport).
/// </summary>
[GlobalClass]
public partial class AudioGlobalPlayer : AudioStreamPlayer
{
    public void PlayGlobal()
    {
        var another = (AudioGlobalPlayer)Duplicate();
        another.Ready += () => another.Play();
        another.Finished += another.QueueFree;
        GetViewport().AddChildSafely(another);
    }
}