using System.Collections.Generic;
using Godot;
using Moon.Utils;

namespace Moon.Class;

[GlobalClass]
public partial class NodePool : Node
{
    [ExportCategory("NodePool")]
    [Export]
    public int PoolSize { get; set; } = 100;
    
    /// <summary>
    /// Make sure the pool objects can init when enter tree.
    /// </summary>
    [Export]
    public PackedScene PoolScene { get; set; }
    
    private Stack<Node> Pool = [];

    public NodePool() : base()
    {
        Ready += () =>
        {
            for (int i = 0; i < PoolSize; i++)
            {
                var node = CreatePoolNode();
                Pool.Push(node);
            }
        };
    }

    public override void _Notification(int what)
    {
        if ((ulong)what == NotificationPredelete)
        {
            foreach (var node in Pool) node.QueueFree();
            Pool.Clear();
        }
    }

    private Node CreatePoolNode()
    {
        var node = PoolScene.InstantiateSafely<Node>();
        SetPool(node);
        
        return node;
    }
    
    public int GetPoolCount() => Pool.Count;

    public Node GetPoolNode()
    {
        if (Pool.Count == 0)
        {
        #if TOOLS
            FD.PushWarning($"NodePool at {this.GetUniquePath()} was running out! Consider increasing the pool size.");
        #endif        
            return CreatePoolNode();
        }
        
        return Pool.Pop();
    }
    
    private void SetPool(Node node) => node.SetData(PoolNodeData, GetInstanceId());
    public void ReturnPool(Node node) => Pool.Push(node);
    
    private const string PoolNodeData = "PoolNode";
    public static NodePool GetPool(Node node)
    {
        if (!node.HasData(PoolNodeData)) return null;
    
        var r = node.GetData<ulong>(PoolNodeData);
        if (IsInstanceIdValid(r)) return (NodePool)InstanceFromId(r);
        
        return null;
    }
}

public static class NodePoolExtensions
{
    /// <summary>
    /// if node is in pool, remove it from parent instead.
    /// </summary>
    public static void TryQueueFree(this Node node)
    {
#if TOOLS
        if (Engine.IsEditorHint())
        {
            FD.PushWarning($"{node} namely {node.GetPathTo(node.GetTree().GetEditedSceneRoot())} is trying to call TryQueueFree in editor, which is not expected.");
        }
#endif
        
        var pool = NodePool.GetPool(node);
        if (pool != null)
        {
            node.GetParent().RemoveChildSafely(node);
            node.Connect(Node.SignalName.TreeExited, Callable.From(() =>
            {
                NodePool.GetPool(node).ReturnPool(node);
            }), (int)GodotObject.ConnectFlags.OneShot);
            return;
        }
        
        node.QueueFree();
    }
}