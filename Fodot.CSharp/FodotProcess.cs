using System;
using Godot;
using Microsoft.FSharp.Core;

namespace Fodot.CSharp;

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
        return Core.Engine.addDeltaProcess(physics, proc.AsFSharpFunc(), node);
    }

    public static Guid AddProcess(this Node node, Action proc, bool physics)
    {
        return Core.Engine.addProcess(physics, proc.AsFSharpFunc(), node);
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
        return Core.Engine.removeProcess(id, node);
    }
}