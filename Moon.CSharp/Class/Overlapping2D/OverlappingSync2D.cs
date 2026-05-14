using Godot;

namespace Moon.Class;

/// <summary>
/// Overlap2D handles overlapping test.
/// Sync shape with specific CollisionObject2D.
/// </summary>
[GlobalClass]
public partial class OverlappingSync2D : Overlapping2D
{

    private OverlapSync2D OverlapObject = new();
    protected override Overlap2D GetOverlapManager() => OverlapObject;

    [ExportCategory("OverlappingSync2D")]
    [Export]
    public CollisionObject2D SyncCollisionObject
    {
        get => _SyncCollisionObject;
        set
        {
            if (_SyncCollisionObject != value)
            {
                if (OverlapObject.SyncObject != null)
                {
                    OverlapObject.RemoveException(OverlapObject.SyncObject);
                }
                
                _SyncCollisionObject = value;
                OverlapObject.SyncObject = value;
                
                if (value != null)
                {
                    OverlapObject.AddException(value);
                }
            }
        }
    }
    private CollisionObject2D _SyncCollisionObject;

    public OverlappingSync2D() : base()
    {
        TreeEntered += () =>
        {
            if (SyncCollisionObject == null) return;
            
            OverlapObject.SyncObject = SyncCollisionObject;
            OverlapObject.AddException(SyncCollisionObject);
        };
    }
}