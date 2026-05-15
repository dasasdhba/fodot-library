using Fodot.Async;
using Godot;
using Moon.Utils;

namespace Moon.Component;

[GlobalClass]
public partial class Node2DCaster : Node
{
	[ExportCategory("Node2DCaster")]
	[Export]
	public Node2D Root { get ;set; }

	[Export]
	public PackedScene Node2DScene { get ;set; }
	
	[Export]
	public Vector2 Offset { get ;set; }
	
	[Export]
	public int BufferCount { get ;set; } = 1;
	
	[Signal]
	public delegate void CastedEventHandler(Node2D node);
	
	public AsyncScene<Node2D> Node2DLoader { get ;set; }

	public Node2DCaster() : base()
	{
		Ready += () =>
		{
			Node2DLoader = AsyncScene.create<Node2D>(Node2DScene, BufferCount, 0, this);
		};
	}

	public void Cast()
	{
		var node = Node2DLoader.Get();
		node.Position = Root.Position + Offset;
		EmitSignal(SignalName.Casted, node);
		Root.AddSiblingSafely(node);
	}
}
