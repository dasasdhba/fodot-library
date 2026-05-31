using Godot;

namespace Fodot.CSharp;

public static class FD
{
    public static void Print(string what)
    {
        Common.Logger.push(what);
    }

    public static void PushWarning(string what)
    {
        Common.Logger.pushWarn(what);
    }
    
    public static void PushError(string what)
    {
        Common.Logger.pushError(what);
    }

    public static T Load<T>(string path) where T : Resource
    {
        return Core.GD.loadAs<T>(path);
    }
}