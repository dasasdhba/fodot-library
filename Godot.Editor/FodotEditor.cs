namespace Godot.Editor;

public static class FodotEditor
{
    public const string MainSceneKey = "fodot/general/main_scene";
    public static string ProjectMainScene => Plugin.GetProjectSetting(MainSceneKey, "");
    
#if DEBUG

    public const string DebugScenePath = "res://.godot/fodot_debug_scene";
    
#endif
}