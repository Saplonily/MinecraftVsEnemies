using MVE.SalExt;

namespace MVE;

public partial class LeftParticle : BoardEntity
{
    public LeftParticle(SalParticleSys sys)
    {
        AddChild(sys);
        sys.AllLifetimeJustEnd += QueueFree;
    }
}
