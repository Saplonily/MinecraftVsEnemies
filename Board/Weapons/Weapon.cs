using Saladim.GodotParticle;

namespace MVE;

public abstract partial class Weapon : LawnEntity
{
    protected Area2D hitBoxArea = null!;
    protected SalParticleSys damagingParticleSys = null!;
    protected bool enableHpLock = true;

    public double MaxHp { get; set; } = 200f;
    public double Hp { get; set; } = 200f;

    public event Action<double>? OnHpChanged;

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
        Hp = Math.Clamp(Hp, 0d, MaxHp);
        if (Hp == 0d)
        {
            OnHpUseUp();
        }
    }

    public virtual void OnPlaced(PlantingArea plantingArea)
    {
        Board.StoneSoundPlayerChooser.Choose().Play();
    }

    public virtual void OnHpUseUp()
    {
        RemoveChild(damagingParticleSys);

        LeftParticle lp = new(damagingParticleSys);
        lp.LevelPos = LevelPos;
        damagingParticleSys.EmitMany(50);

        Board.StoneSoundPlayerChooser.Choose().Play();

        Lawn.AddChild(lp);
        QueueFree();
        OnDestroyed?.Invoke();
    }

    public virtual void BeHurt(LawnEntity sourceEntity, double amount)
    {
        if (Game.OnPhysicsInterval(8))
            damagingParticleSys.Emit();
        Hp -= amount;
        OnHpChanged?.Invoke(-amount);
    }
}