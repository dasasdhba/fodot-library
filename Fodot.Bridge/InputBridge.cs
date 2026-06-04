using System;
using Godot;

namespace Fodot.Bridge;

public partial class InputBridge : Node
{
    public event Action<InputEvent> Input;

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);
        Input?.Invoke(@event);
    }
}