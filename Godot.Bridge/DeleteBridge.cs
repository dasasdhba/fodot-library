using System;
using Godot;

namespace GodotBridge;

public partial class DeleteBridge : Node
{
    public event Action Deleted;

    public override void _Notification(int what)
    {
        if ((ulong)what == NotificationPredelete)
        {
            Deleted?.Invoke();
        }
    }
}