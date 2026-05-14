using System;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using Microsoft.FSharp.Core;

namespace Moon.Utils;

public static class FodotProcess
{
    // fsharp functions

    public static FSharpFunc<TU, TV> AsFSharpFunc<TU, TV>(this Func<TU, TV> func)
    {
        return FuncConvert.ToFSharpFunc<TU, TV>(func.Invoke);
    }
    
    public static FSharpFunc<Unit, T> AsFSharpFunc<T>(this Func<T> func)
    {
        return FuncConvert.ToFSharpFunc<Unit, T>(unit => func());
    }
    
    public static FSharpFunc<T, Unit> AsFSharpFunc<T>(this Action<T> action)
    {
        return FuncConvert.ToFSharpFunc<T, Unit>(t => 
        {
            action.Invoke(t);
            return null;
        });
    }
    
    public static FSharpFunc<Unit, Unit> AsFSharpFunc(this Action action)
    {
        return FuncConvert.ToFSharpFunc<Unit, Unit>(t => 
        {
            action.Invoke();
            return null;
        });
    }
    
    // fodot process

    public static Guid AddProcess(this Node node, Action<double> proc, bool physics)
    {
        return Fodot.Core.Engine.addDeltaProcess(proc.AsFSharpFunc(), physics, node);
    }

    public static Guid AddProcess(this Node node, Action proc, bool physics)
    {
        return Fodot.Core.Engine.addProcess(proc.AsFSharpFunc(), physics, node);
    }

    public static Guid AddIdleProcess(this Node node, Action<double> proc)
    {
        return node.AddProcess(proc, false);
    }
    
    public static Guid AddIdleProcess(this Node node, Action proc)
    {
        return node.AddProcess(proc, false);
    }
    
    public static Guid AddPhysicsProcess(this Node node, Action<double> proc)
    {
        return node.AddProcess(proc, true);
    }
    
    public static Guid AddPhysicsProcess(this Node node, Action proc)
    {
        return node.AddProcess(proc, true);
    }

    public static bool RemoveProcess(this Node node, Guid id)
    {
        return Fodot.Core.Engine.removeProcess(id, node);
    }
    
    // action

    public static Guid ActionRepeat(this Node node, double interval, Action action, bool physics = false)
    {
        return Module.Action.repeat(interval, action.AsFSharpFunc(), physics, node);
    }

    public static Guid ActionRepeatIdle(this Node node, double interval, Action action)
    {
        return node.ActionRepeat(interval, action, false);
    }
    
    public static Guid ActionRepeatPhysics(this Node node, double interval, Action action)
    {
        return node.ActionRepeat(interval, action, true);
    }

    public static Task<Unit> ActionDelay(this Node node, double interval, Action action, bool physics = false)
    {
        return Module.Action.delay(interval, action.AsFSharpFunc(), physics, node);
    }
    
    public static Task<Unit> ActionDelayIdle(this Node node, double interval, Action action)
    {
        return node.ActionDelay(interval, action, false);
    }
    
    public static Task<Unit> ActionDelayPhysics(this Node node, double interval, Action action)
    {
        return node.ActionDelay(interval, action, true);
    }
    
    public static Task<Unit> ActionDelay(this Node node, double interval, Action action, CancellationToken ct, bool physics = false)
    {
        return Module.Action.delayWith(ct, interval, action.AsFSharpFunc(), physics, node);
    }
    
    public static Task<Unit> ActionDelayIdle(this Node node, double interval, Action action, CancellationToken ct)
    {
        return node.ActionDelay(interval, action, ct, false);
    }
    
    public static Task<Unit> ActionDelayPhysics(this Node node, double interval, Action action, CancellationToken ct)
    {
        return node.ActionDelay(interval, action, ct, true);
    }
}