using MVE.SalExt;

namespace MVE;

public abstract partial class Weapon : LawnEntity
{
    protected Sprite2D sprite = default!;
    protected Area2D hitBoxArea = default!;
    protected SalParticleSys damagingParticleSys = default!;

    protected bool onPickingLighted = false;

    protected bool enableHpLock = true;

    public Tag<int> Tag;
    public double MaxHp { get; set; } = 200f;
    public double Hp { get; set; } = 200f;

    public event Action<double>? HpChanged;
    public event Action<Weapon, DeathReason>? Died;

    public override void _Ready()
    {
        base._Ready();
        Tag = 0;
        hitBoxArea = GetNode<Area2D>("HitArea") ?? throw new NodeNotFoundException("HitArea");
        damagingParticleSys = GetNode<SalParticleSys>("DamagingParticleSys") ??
            throw new NodeNotFoundException("DamagingParticleSys");
        sprite = GetNode<Sprite2D>("Sprite") ?? throw new NodeNotFoundException("Sprite");


        hitBoxArea.MouseEntered += () =>
        {
            if (Board.Picking is PickingType.Pickaxe)
                MakeLight();
            Board.PickingChanged += EnterLightCheck;
        };
        hitBoxArea.MouseExited += () =>
        {
            Board.PickingChanged -= EnterLightCheck;
            if (Board.Picking is PickingType.Pickaxe)
                RestoreLight();
        };
        hitBoxArea.InputEvent += HitBoxArea_InputEvent;
    }

    protected void EnterLightCheck(PickingType from, PickingType to)
    {
        if (from is not PickingType.Pickaxe && to is PickingType.Pickaxe)
            MakeLight();
        if (from is PickingType.Pickaxe && to is not PickingType.Pickaxe)
            RestoreLight();
    }

    protected void RestoreLight()
        => (sprite.Modulate, onPickingLighted) = (sprite.Modulate / 1.5f, false);
    protected void MakeLight()
        => (sprite.Modulate, onPickingLighted) = (sprite.Modulate * 1.5f, true);

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        Board.PickingChanged -= EnterLightCheck;
    }

    protected void HitBoxArea_InputEvent(Node viewport, InputEvent ie, long shapeIdx)
    {
        if (Board.Picking is PickingType.Pickaxe && ie.IsActionPressed(InputNames.Using))
        {
            if (Board.PickedNode is Pickaxe pickaxe)
            {
                Hp = 0d;
                pickaxe.OnUsed();
            }
        }
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        Hp = Math.Clamp(Hp, 0d, MaxHp);
        if (Hp == 0)
        {
            Die(DeathReason.None);
        }
    }

    public virtual void OnPlaced(PlantingArea plantingArea)
    {
        Board.StoneSoundPlayerChooser.Choose().Play();
    }

    public virtual void Die(DeathReason deathReason)
    {
        Board.StoneSoundPlayerChooser.Choose().Play();

        RemoveChild(damagingParticleSys);
        LeftParticle lp = new(damagingParticleSys);
        lp.LevelPos = LevelPos;
        damagingParticleSys.EmitMany(50); Lawn.AddChild(lp);
        Died?.Invoke(this, deathReason);
        QueueFree();
    }

    public virtual void BeHurt(double amount /*TODO: more arguments*/ )
    {
        if (Game.OnPhysicsInterval(8))
            damagingParticleSys.Emit();
        Hp -= amount;
        HpChanged?.Invoke(-amount);
    }
}
