using Saladim.GodotParticle;

namespace MVE;

public abstract partial class Weapon : LawnEntity
{
    protected double maxHp = 200f;
    protected double hp = 200f;
    protected Area2D hitBoxArea = null!;
    protected SalParticleSys damagingParticleSys = null!;
    protected bool enableHpLock = true;

    public event Action? OnDestroyed;

    public override void _Ready()
    {
        base._Ready();
        hitBoxArea = GetNode<Area2D>("HitArea") ?? throw new NodeNotFoundException("HitArea");

        damagingParticleSys = GetNode<SalParticleSys>("DamagingParticleSys") ??
            throw new NodeNotFoundException("DamagingParticleSys");
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        hp = Math.Clamp(hp, 0d, maxHp);
        if (hp == 0d)
        {
            OnHpUseUp();
        }
    }

    public virtual void OnHpUseUp()
    {
        RemoveChild(damagingParticleSys);

        LeftParticle lp = new(damagingParticleSys);
        lp.LevelPos = LevelPos;
        damagingParticleSys.EmitMany(50);

        Lawn.AddChild(lp);
        QueueFree();
        OnDestroyed?.Invoke();
    }

    public virtual void BeHurt(LawnEntity sourceEntity, double amount)
    {
        if (Game.OnPhysicsInterval(8))
            damagingParticleSys.Emit();
        hp -= amount;
    }
}