using Godot;
using Moon.Utils;

namespace Moon.Class;

/// <summary>
/// Play audio in global scope(viewport) then free.
/// </summary>
[GlobalClass]
public partial class AudioOneshotPlayer : AudioStreamPlayer
{
    public AudioOneshotPlayer() :base()
    {
        Finished += QueueFree;
    }

    public void PlayOneshot()
    {
        this.ReparentSafely(GetViewport());
        Play();
    }
}