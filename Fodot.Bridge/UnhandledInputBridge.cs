using System;
using Godot;

namespace Fodot.Bridge;

public partial class UnhandledInputBridge : Node
{
    public event Action<InputEvent> UnhandledInput;

    public override void _UnhandledInput(InputEvent @event)
    {
        base._UnhandledInput(@event);
        UnhandledInput?.Invoke(@event);
    }
}