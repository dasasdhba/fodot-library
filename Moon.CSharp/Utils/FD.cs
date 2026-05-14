using Godot;

namespace Moon.Utils;

public static class FD
{
    public static void Print(string what)
    {
        Fodot.Common.Logger.push(what);
    }

    public static void PushWarning(string what)
    {
        Fodot.Common.Logger.pushWarn(what);
    }
    
    public static void PushError(string what)
    {
        Fodot.Common.Logger.pushError(what);
    }

    public static T Load<T>(string path) where T : Resource
    {
        return Fodot.Core.GD.loadAs<T>(path);
    }
}