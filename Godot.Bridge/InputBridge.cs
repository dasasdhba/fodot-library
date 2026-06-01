using System;
using Godot;

namespace GodotBridge;

public partial class InputBridge : Node
{
    public event Action<InputEvent> Input;

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);
        Input?.Invoke(@event);
    }
}