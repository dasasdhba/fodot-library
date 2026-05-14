using System.Collections.Generic;
using Godot;
using Moon.Utils;

namespace Moon.Class;

[GlobalClass]
public partial class InputCustom : Inputer
{
    public enum InputCustomProcessCallback
    {
        Idle,
        Physics
    }

    [ExportCategory("InputCustom")]
    [Export]
    public InputCustomProcessCallback CustomProcessMode { get; set; }
        = InputCustomProcessCallback.Physics;

    protected Dictionary<string, InputKey> InputKeyMap = new();
    protected Dictionary<string, bool> SetInputMap = new();

    public override InputKey GetKey(string key)
    {
        if (!InputKeyMap.ContainsKey(key)) return new();
        return InputKeyMap[key];
    }

    public void SetKey(string key, bool pressed)
    {
        if (!SetInputMap.TryAdd(key, pressed)) SetInputMap[key] = pressed;
    }

    public void ClearKey()
    {
        InputKeyMap.Clear();
        SetInputMap.Clear();
    }

    public InputCustom() :base()
    {
        Ready += () => this.AddProcess(InputProcess, CustomProcessMode == InputCustomProcessCallback.Physics);
    }
    
    public void InputProcess()
    {
        foreach (var key in InputKeyMap.Keys)
        {
            InputKeyMap[key] = new(InputKeyMap[key].Pressed, false, false);
        }

        foreach (var key in SetInputMap.Keys)
        {
            var pressed = SetInputMap[key];
            if (InputKeyMap.ContainsKey(key))
            {
                InputKeyMap[key] = pressed ?
                    new(true, !InputKeyMap[key].Pressed, false) :
                    new(false, false, InputKeyMap[key].Pressed);
            }
            else
            {
                InputKeyMap[key] = pressed ? new(true, true, false) : new();
            }
        }

        SetInputMap.Clear();
    }
}