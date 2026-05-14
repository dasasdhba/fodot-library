using Godot;
using Moon.Utils;

namespace Moon.Class;

public partial class DrawParticles2D : DrawProcess2D
{
    [ExportCategory("DrawParticles2D")]
    [Export]
    public bool Emitting { get; set; } = false;
    
    [Export(PropertyHint.Range, "0.001,4096,or_greater")]
    public double Interval { get ;set; } = 0.02d;
    
    /// <summary>
    /// Make AddDrawProcess call here.
    /// </summary>
    protected virtual void ParticleSetup()
    {
        
    }

    public DrawParticles2D() : base()
    {
        Ready += () =>
        {
            this.ActionRepeat(Interval, () =>
            {
                if (Emitting)
                {
                    ParticleSetup();
                }
            },  ProcessCallback == Draw2DProcessCallback.Physics);
        };
    }
}