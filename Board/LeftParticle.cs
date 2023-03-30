using Saladim.GodotParticle;

namespace MVE;

public partial class LeftParticle : BoardEntity
{
    public LeftParticle(SalParticleSys sys)
    {
        AddChild(sys);
        sys.AllLifetimeJustEnd += QueueFree;
    }
}
