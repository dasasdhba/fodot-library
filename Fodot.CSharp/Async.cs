using System;
using System.Threading;
using System.Threading.Tasks;
using FSharp;
using Godot;
using Microsoft.FSharp.Core;

namespace Fodot.CSharp;

/// <summary>
/// C# wrappers for AsyncNode.
/// </summary>
public static class AsyncExtensions
{
    private static Async.AsyncNode NewAsyncNode(this Node node, bool physics, CancellationToken ct = default)
        => Async.AsyncNode.New(node, physics, ct);

    private static Task CompletedIfInvalid(this Node node, Func<Task> task)
        => GodotObject.IsInstanceValid(node) ? task() : Task.CompletedTask;

    private static Task<T> CompletedIfInvalid<T>(this Node node, Func<Task<T>> task)
        => GodotObject.IsInstanceValid(node) ? task() : Task.FromResult(default(T));

    private static ProcessFunc<Unit> Proc(Action action)
        => ProcessFunc<Unit>.NewUnit(action.AsFSharpFunc());

    private static ProcessFunc<Unit> Proc(Action<double> action)
        => ProcessFunc<Unit>.NewDelta(action.AsFSharpFunc());

    private static ProcessFunc<bool> Predicate(Func<bool> action)
        => ProcessFunc<bool>.NewUnit(action.AsFSharpFunc());

    private static ProcessFunc<bool> Predicate(Func<double, bool> action)
        => ProcessFunc<bool>.NewDelta(action.AsFSharpFunc());

    public static Task Await(this Node node, double time, bool physics = false)
        => node.CompletedIfInvalid(() => node.NewAsyncNode(physics).Delay(time));

    public static Task Await(this Node node, double time, CancellationToken ct, bool physics = false)
        => node.CompletedIfInvalid(() => node.NewAsyncNode(physics, ct).Delay(time));

    public static Task AwaitPhysics(this Node node, double time)
        => node.Await(time, true);

    public static Task AwaitPhysics(this Node node, double time, CancellationToken ct)
        => node.Await(time, ct, true);

    public static Task AwaitProcess(this Node node, double time, Action<double> process, bool physics = false)
        => node.CompletedIfInvalid(() => node.NewAsyncNode(physics).DelayWith(Proc(process), time));

    public static Task AwaitProcess(this Node node, double time, Action<double> process, CancellationToken ct, bool physics = false)
        => node.CompletedIfInvalid(() => node.NewAsyncNode(physics, ct).DelayWith(Proc(process), time));

    public static Task AwaitPhysicsProcess(this Node node, double time, Action<double> process)
        => node.AwaitProcess(time, process, true);

    public static Task AwaitPhysicsProcess(this Node node, double time, Action<double> process, CancellationToken ct)
        => node.AwaitProcess(time, process, ct, true);

    public static Task AwaitProcess(this Node node, double time, Action process, bool physics = false)
        => node.CompletedIfInvalid(() => node.NewAsyncNode(physics).DelayWith(Proc(process), time));

    public static Task AwaitProcess(this Node node, double time, Action process, CancellationToken ct, bool physics = false)
        => node.CompletedIfInvalid(() => node.NewAsyncNode(physics, ct).DelayWith(Proc(process), time));

    public static Task AwaitPhysicsProcess(this Node node, double time, Action process)
        => node.AwaitProcess(time, process, true);

    public static Task AwaitPhysicsProcess(this Node node, double time, Action process, CancellationToken ct)
        => node.AwaitProcess(time, process, ct, true);

    public static Task AwaitUntil(this Node node, Func<double, bool> action, bool physics = false)
        => node.CompletedIfInvalid(() => node.NewAsyncNode(physics).Until(Predicate(action)));

    public static Task AwaitUntil(this Node node, Func<double, bool> action, CancellationToken ct, bool physics = false)
        => node.CompletedIfInvalid(() => node.NewAsyncNode(physics, ct).Until(Predicate(action)));

    public static Task AwaitUntilPhysics(this Node node, Func<double, bool> action)
        => node.AwaitUntil(action, true);

    public static Task AwaitUntilPhysics(this Node node, Func<double, bool> action, CancellationToken ct)
        => node.AwaitUntil(action, ct, true);

    public static Task AwaitUntil(this Node node, Func<bool> action, bool physics = false)
        => node.CompletedIfInvalid(() => node.NewAsyncNode(physics).Until(Predicate(action)));

    public static Task AwaitUntil(this Node node, Func<bool> action, CancellationToken ct, bool physics = false)
        => node.CompletedIfInvalid(() => node.NewAsyncNode(physics, ct).Until(Predicate(action)));

    public static Task AwaitUntilPhysics(this Node node, Func<bool> action)
        => node.AwaitUntil(action, true);

    public static Task AwaitUntilPhysics(this Node node, Func<bool> action, CancellationToken ct)
        => node.AwaitUntil(action, ct, true);

    public static Task AwaitFrame(this Node node, int frames, bool physics = false)
        => node.CompletedIfInvalid(() => node.NewAsyncNode(physics).DelayFrame((uint)frames));

    public static Task AwaitFrame(this Node node, bool physics = false)
        => node.AwaitFrame(1, physics);

    public static Task AwaitFrame(this Node node, int frames, CancellationToken ct, bool physics = false)
        => node.CompletedIfInvalid(() => node.NewAsyncNode(physics, ct).DelayFrame((uint)frames));

    public static Task AwaitFrame(this Node node, CancellationToken ct, bool physics = false)
        => node.AwaitFrame(1, ct, physics);

    public static Task AwaitPhysicsFrame(this Node node, int frames)
        => node.AwaitFrame(frames, true);

    public static Task AwaitPhysicsFrame(this Node node, bool physics = false)
        => node.AwaitFrame(1, true);

    public static Task AwaitPhysicsFrame(this Node node, int frames, CancellationToken ct)
        => node.AwaitFrame(frames, ct, true);

    public static Task AwaitPhysicsFrame(this Node node, CancellationToken ct, bool physics = false)
        => node.AwaitFrame(1, ct, true);

    public static Task Await(this Node node, Task task, bool physics = false)
        => node.CompletedIfInvalid(() => node.NewAsyncNode(physics).Wait(task.AsUnit()));

    public static Task Await(this Node node, Task task, CancellationToken ct, bool physics = false)
        => node.CompletedIfInvalid(() => node.NewAsyncNode(physics, ct).Wait(task.AsUnit()));

    public static Task<T> Await<T>(this Node node, Task<T> task, bool physics = false)
        => node.CompletedIfInvalid(() => node.NewAsyncNode(physics).Wait(task));

    public static Task<T> Await<T>(this Node node, Task<T> task, CancellationToken ct, bool physics = false)
        => node.CompletedIfInvalid(() => node.NewAsyncNode(physics, ct).Wait(task));

    public static Task AwaitPhysics(this Node node, Task task)
        => node.Await(task, true);

    public static Task AwaitPhysics(this Node node, Task task, CancellationToken ct)
        => node.Await(task, ct, true);

    public static Task<T> AwaitPhysics<T>(this Node node, Task<T> task)
        => node.Await(task, true);

    public static Task<T> AwaitPhysics<T>(this Node node, Task<T> task, CancellationToken ct)
        => node.Await(task, ct, true);

    public static Task AwaitProcess(this Node node, Task task, Action<double> process, bool physics = false)
        => node.CompletedIfInvalid(() => node.NewAsyncNode(physics).WaitWith(Proc(process), task.AsUnit()));

    public static Task AwaitProcess(this Node node, Task task, Action<double> process, CancellationToken ct, bool physics = false)
        => node.CompletedIfInvalid(() => node.NewAsyncNode(physics, ct).WaitWith(Proc(process), task.AsUnit()));

    public static Task<T> AwaitProcess<T>(this Node node, Task<T> task, Action<double> process, bool physics = false)
        => node.CompletedIfInvalid(() => node.NewAsyncNode(physics).WaitWith(Proc(process), task));

    public static Task<T> AwaitProcess<T>(this Node node, Task<T> task, Action<double> process, CancellationToken ct, bool physics = false)
        => node.CompletedIfInvalid(() => node.NewAsyncNode(physics, ct).WaitWith(Proc(process), task));

    public static Task AwaitPhysicsProcess(this Node node, Task task, Action<double> process)
        => node.AwaitProcess(task, process, true);

    public static Task AwaitPhysicsProcess(this Node node, Task task, Action<double> process, CancellationToken ct)
        => node.AwaitProcess(task, process, ct, true);

    public static Task<T> AwaitPhysicsProcess<T>(this Node node, Task<T> task, Action<double> process)
        => node.AwaitProcess(task, process, true);

    public static Task<T> AwaitPhysicsProcess<T>(this Node node, Task<T> task, Action<double> process, CancellationToken ct)
        => node.AwaitProcess(task, process, ct, true);

    public static Task AwaitProcess(this Node node, Task task, Action process, bool physics = false)
        => node.CompletedIfInvalid(() => node.NewAsyncNode(physics).WaitWith(Proc(process), task.AsUnit()));

    public static Task AwaitProcess(this Node node, Task task, Action process, CancellationToken ct, bool physics = false)
        => node.CompletedIfInvalid(() => node.NewAsyncNode(physics, ct).WaitWith(Proc(process), task.AsUnit()));

    public static Task<T> AwaitProcess<T>(this Node node, Task<T> task, Action process, bool physics = false)
        => node.CompletedIfInvalid(() => node.NewAsyncNode(physics).WaitWith(Proc(process), task));

    public static Task<T> AwaitProcess<T>(this Node node, Task<T> task, Action process, CancellationToken ct, bool physics = false)
        => node.CompletedIfInvalid(() => node.NewAsyncNode(physics, ct).WaitWith(Proc(process), task));

    public static Task AwaitPhysicsProcess(this Node node, Task task, Action process)
        => node.AwaitProcess(task, process, true);

    public static Task AwaitPhysicsProcess(this Node node, Task task, Action process, CancellationToken ct)
        => node.AwaitProcess(task, process, ct, true);

    public static Task<T> AwaitPhysicsProcess<T>(this Node node, Task<T> task, Action process)
        => node.AwaitProcess(task, process, true);

    public static Task<T> AwaitPhysicsProcess<T>(this Node node, Task<T> task, Action process, CancellationToken ct)
        => node.AwaitProcess(task, process, ct, true);

    public static Task Await(this Node node, GodotObject obj, StringName signal, bool physics = false)
        => node.CompletedIfInvalid(() => node.NewAsyncNode(physics).WaitSignal<Unit>(obj, signal).AsUnit());

    public static Task Await(this Node node, GodotObject obj, StringName signal, CancellationToken ct, bool physics = false)
        => node.CompletedIfInvalid(() => node.NewAsyncNode(physics, ct).WaitSignal<Unit>(obj, signal).AsUnit());

    public static Task<T> Await<T>(this Node node, GodotObject obj, StringName signal, bool physics = false)
        => node.CompletedIfInvalid(() => node.NewAsyncNode(physics).WaitSignal<T>(obj, signal));

    public static Task<T> Await<T>(this Node node, GodotObject obj, StringName signal, CancellationToken ct, bool physics = false)
        => node.CompletedIfInvalid(() => node.NewAsyncNode(physics, ct).WaitSignal<T>(obj, signal));

    public static Task AwaitPhysics(this Node node, GodotObject obj, StringName signal)
        => node.Await(obj, signal, true);

    public static Task AwaitPhysics(this Node node, GodotObject obj, StringName signal, CancellationToken ct)
        => node.Await(obj, signal, ct, true);

    public static Task<T> AwaitPhysics<T>(this Node node, GodotObject obj, StringName signal)
        => node.Await<T>(obj, signal, true);

    public static Task<T> AwaitPhysics<T>(this Node node, GodotObject obj, StringName signal, CancellationToken ct)
        => node.Await<T>(obj, signal, ct, true);

    public static Task AwaitProcess(this Node node, GodotObject obj, StringName signal, Action<double> process, bool physics = false)
        => node.CompletedIfInvalid(() => node.NewAsyncNode(physics).WaitSignalWith<Unit>(Proc(process), obj, signal).AsUnit());

    public static Task AwaitProcess(this Node node, GodotObject obj, StringName signal, Action<double> process, CancellationToken ct, bool physics = false)
        => node.CompletedIfInvalid(() => node.NewAsyncNode(physics, ct).WaitSignalWith<Unit>(Proc(process), obj, signal).AsUnit());

    public static Task<T> AwaitProcess<T>(this Node node, GodotObject obj, StringName signal, Action<double> process, bool physics = false)
        => node.CompletedIfInvalid(() => node.NewAsyncNode(physics).WaitSignalWith<T>(Proc(process), obj, signal));

    public static Task<T> AwaitProcess<T>(this Node node, GodotObject obj, StringName signal, Action<double> process, CancellationToken ct, bool physics = false)
        => node.CompletedIfInvalid(() => node.NewAsyncNode(physics, ct).WaitSignalWith<T>(Proc(process), obj, signal));

    public static Task AwaitPhysicsProcess(this Node node, GodotObject obj, StringName signal, Action<double> process)
        => node.AwaitProcess(obj, signal, process, true);

    public static Task AwaitPhysicsProcess(this Node node, GodotObject obj, StringName signal, Action<double> process, CancellationToken ct)
        => node.AwaitProcess(obj, signal, process, ct, true);

    public static Task<T> AwaitPhysicsProcess<T>(this Node node, GodotObject obj, StringName signal, Action<double> process)
        => node.AwaitProcess<T>(obj, signal, process, true);

    public static Task<T> AwaitPhysicsProcess<T>(this Node node, GodotObject obj, StringName signal, Action<double> process, CancellationToken ct)
        => node.AwaitProcess<T>(obj, signal, process, ct, true);

    public static Task AwaitProcess(this Node node, GodotObject obj, StringName signal, Action process, bool physics = false)
        => node.CompletedIfInvalid(() => node.NewAsyncNode(physics).WaitSignalWith<Unit>(Proc(process), obj, signal).AsUnit());

    public static Task AwaitProcess(this Node node, GodotObject obj, StringName signal, Action process, CancellationToken ct, bool physics = false)
        => node.CompletedIfInvalid(() => node.NewAsyncNode(physics, ct).WaitSignalWith<Unit>(Proc(process), obj, signal).AsUnit());

    public static Task<T> AwaitProcess<T>(this Node node, GodotObject obj, StringName signal, Action process, bool physics = false)
        => node.CompletedIfInvalid(() => node.NewAsyncNode(physics).WaitSignalWith<T>(Proc(process), obj, signal));

    public static Task<T> AwaitProcess<T>(this Node node, GodotObject obj, StringName signal, Action process, CancellationToken ct, bool physics = false)
        => node.CompletedIfInvalid(() => node.NewAsyncNode(physics, ct).WaitSignalWith<T>(Proc(process), obj, signal));

    public static Task AwaitPhysicsProcess(this Node node, GodotObject obj, StringName signal, Action process)
        => node.AwaitProcess(obj, signal, process, true);

    public static Task AwaitPhysicsProcess(this Node node, GodotObject obj, StringName signal, Action process, CancellationToken ct)
        => node.AwaitProcess(obj, signal, process, ct, true);

    public static Task<T> AwaitPhysicsProcess<T>(this Node node, GodotObject obj, StringName signal, Action process)
        => node.AwaitProcess<T>(obj, signal, process, true);

    public static Task<T> AwaitPhysicsProcess<T>(this Node node, GodotObject obj, StringName signal, Action process, CancellationToken ct)
        => node.AwaitProcess<T>(obj, signal, process, ct, true);

    public static Task AwaitProcess(this Node node, Tween tween, Action<double> process, bool physics = false)
        => node.CompletedIfInvalid(() => node.NewAsyncNode(physics).WaitTweenWith(Proc(process), tween));

    public static Task AwaitProcess(this Node node, Tween tween, Action<double> process, CancellationToken ct, bool physics = false)
        => node.CompletedIfInvalid(() => node.NewAsyncNode(physics, ct).WaitTweenWith(Proc(process), tween));

    public static Task AwaitPhysicsProcess(this Node node, Tween tween, Action<double> process)
        => node.AwaitProcess(tween, process, true);

    public static Task AwaitPhysicsProcess(this Node node, Tween tween, Action<double> process, CancellationToken ct)
        => node.AwaitProcess(tween, process, ct, true);

    public static Task AwaitProcess(this Node node, Tween tween, Action process, bool physics = false)
        => node.AwaitProcess(tween, delta => process.Invoke(), physics);

    public static Task AwaitProcess(this Node node, Tween tween, Action process, CancellationToken ct, bool physics = false)
        => node.AwaitProcess(tween, delta => process.Invoke(), ct, physics);

    public static Task AwaitPhysicsProcess(this Node node, Tween tween, Action process)
        => node.AwaitProcess(tween, process, true);

    public static Task AwaitPhysicsProcess(this Node node, Tween tween, Action process, CancellationToken ct)
        => node.AwaitProcess(tween, process, ct, true);

    public static Task Await(this Node node, Tween tween, bool physics = false)
        => node.CompletedIfInvalid(() => node.NewAsyncNode(physics).WaitTween(tween));

    public static Task Await(this Node node, Tween tween, CancellationToken ct, bool physics = false)
        => node.CompletedIfInvalid(() => node.NewAsyncNode(physics, ct).WaitTween(tween));

    public static Task AwaitPhysics(this Node node, Tween tween)
        => node.Await(tween, true);

    public static Task AwaitPhysics(this Node node, Tween tween, CancellationToken ct)
        => node.Await(tween, ct, true);

    public static Task AwaitRepeat(this Node node, double time, int count, Action<int> action, bool physics = false)
    {
        var counter = 0;
        return node.AwaitRepeat(time, count, () => action?.Invoke(counter++), physics);
    }

    public static Task AwaitRepeat(this Node node, double time, int count, Action<int> action, CancellationToken ct, bool physics = false)
    {
        var counter = 0;
        return node.AwaitRepeat(time, count, () => action?.Invoke(counter++), ct, physics);
    }

    public static Task AwaitRepeat(this Node node, double time, int count, Action action, bool physics = false)
        => node.CompletedIfInvalid(() => node.NewAsyncNode(physics).Repeat(time, (uint)Math.Max(0, count), action.AsFSharpFunc()));

    public static Task AwaitRepeat(this Node node, double time, int count, Action action, CancellationToken ct, bool physics = false)
        => node.CompletedIfInvalid(() => node.NewAsyncNode(physics, ct).Repeat(time, (uint)Math.Max(0, count), action.AsFSharpFunc()));

    public static Task AwaitRepeatPhysics(this Node node, double time, int count, Action<int> action)
        => node.AwaitRepeat(time, count, action, true);

    public static Task AwaitRepeatPhysics(this Node node, double time, int count, Action<int> action, CancellationToken ct)
        => node.AwaitRepeat(time, count, action, ct, true);

    public static Task AwaitRepeatPhysics(this Node node, double time, int count, Action action)
        => node.AwaitRepeat(time, count, action, true);

    public static Task AwaitRepeatPhysics(this Node node, double time, int count, Action action, CancellationToken ct)
        => node.AwaitRepeat(time, count, action, ct, true);
}
