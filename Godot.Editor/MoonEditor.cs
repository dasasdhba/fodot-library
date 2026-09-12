namespace Godot.Editor;

public static class MoonEditor
{
    public const string MainSceneKey = "moon/general/main_scene";
    public static string ProjectMainScene => Plugin.GetProjectSetting(MainSceneKey, "");
    
#if DEBUG

    public const string DebugScenePath = "res://.godot/moon_debug_scene";
    
#endif
}
