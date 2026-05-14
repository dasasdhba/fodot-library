using System;
using System.Collections.Generic;

namespace Moon.Component;

/// <summary>
/// behavior disable management
/// </summary>
public class DisableList
{
    private HashSet<string> List = [];
    
    public Action OnDisabled { private get; init; }
    public Action OnEnabled { private get; init; }

    public void Disable(string reason)
    {
        if (List.Count == 0)
        {
            OnDisabled?.Invoke();
        }
        List.Add(reason);
    }
    
    public void Enable(string reason)
    {
        List.Remove(reason);
        if (List.Count == 0)
        {
            OnEnabled?.Invoke();
        }
    }
    
    public bool IsDisabled() => List.Count > 0;
    
    public bool HasReason(string reason) => List.Contains(reason);
    public void Clear() => List.Clear();
}