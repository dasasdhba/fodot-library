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
}
