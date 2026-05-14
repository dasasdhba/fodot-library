using Godot;

namespace Moon.Class;

/// <summary>
/// Overlap3D handles overlapping test.
/// Sync shape with specific CollisionObject3D.
/// </summary>
[GlobalClass]
public partial class OverlappingSync3D :
 Overlapping3D
{

    private OverlapSync3D OverlapObject = new();
    protected override Overlap3D GetOverlapManager() => OverlapObject;

    [ExportCategory("OverlappingSync3D")]
    [Export]
    public CollisionObject3D SyncCollisionObject
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
    private CollisionObject3D _SyncCollisionObject;

    public OverlappingSync3D() : base()
    {
        TreeEntered += () =>
        {
            if (SyncCollisionObject == null) return;
            
            OverlapObject.SyncObject = SyncCollisionObject;
            OverlapObject.AddException(SyncCollisionObject);
        };
    }
}