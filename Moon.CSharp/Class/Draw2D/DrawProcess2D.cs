using System;
using System.Collections.Generic;

namespace Moon.Class;

/// <summary>
/// Manage drawing task with process functions.
/// </summary>
public partial class DrawProcess2D : Draw2D
{    
    private List<Func<double, bool>> QueuedTasks = [];
    private List<Func<double, bool>> FinishedTasks = [];
    
    public void AddDrawProcess(Func<double, bool> process)
        => QueuedTasks.Add(process);

    protected override bool DrawProcess(double delta)
    {
        foreach (var task in QueuedTasks)
        {
            ClearDrawSettings();
            
            var result = task(delta);
            if (result)
            {
                FinishedTasks.Add(task);
            }
        }
        
        foreach (var task in FinishedTasks)
        {
            QueuedTasks.Remove(task);
        }
        
        FinishedTasks.Clear();
        
        return true;
    }
}