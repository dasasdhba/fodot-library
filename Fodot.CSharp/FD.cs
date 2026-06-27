using System;
using Godot;

namespace Fodot.CSharp;

public static class FD
{
    public static void Print(string what)
    {
        Logger.push(what);
    }

    public static void PushWarning(string what)
    {
        Logger.pushWarn(what);
    }
    
    public static void PushError(string what)
    {
        Logger.pushError(what);
    }

    public static T Load<T>(string path) where T : Resource
    {
        return GDModule.loadAs<T>(path);
    }

    public static void Post(Action action)
    {
        GDThread.post(action.AsFSharpFunc());
    }

    public static void Send(Action action)
    {
        GDThread.send(action.AsFSharpFunc());
    }
}
