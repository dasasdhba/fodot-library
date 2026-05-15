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
    
    private Library.NodePool pool;

    public NodePool() : base()
    {
        Ready += () =>
        {
            pool = Library.NodePoolModule.create(PoolScene, PoolSize, this);
        };
    }
    
    public Node GetPoolNode()
    {
        return pool.Get();
    }

    public T GetPoolNode<T>() where T : Node
    {
        return pool.GetAs<T>();
    }
}

public static class NodePoolExtensions
{
    /// <summary>
    /// if node is in pool, remove it from parent instead.
    /// </summary>
    public static bool TryQueueFree(this Node node)
    {
#if TOOLS
        if (Engine.IsEditorHint())
        {
            FD.PushWarning($"{node} namely {node.GetPathTo(node.GetTree().GetEditedSceneRoot())} is trying to call TryQueueFree in editor, which is not expected.");
            return false;
        }
#endif
        
        return Library.NodePoolModule.returnPool(node);
    }
}