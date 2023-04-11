using MVE.SalExt;

namespace MVE;

public abstract partial class Weapon : LawnEntity
{
    protected Sprite2D sprite = default!;
    protected Area2D hitBoxArea = default!;
    protected SalParticleSys damagingParticleSys = default!;

    protected bool onPickingLighted = false;

    protected bool enableHpLock = true;

    public double MaxHp { get; set; } = 200f;
    public double Hp { get; set; } = 200f;

    public event Action<double>? HpChanged;
    public event Action? Destroyed;

    public override void _Ready()
    {
        base._Ready();
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
        hitBoxArea.InputEvent += this.HitBoxArea_InputEvent;
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
        if (Hp == 0d)
        {
            OnHpUseUp();
        }
    }

    public virtual void OnPlaced(PlantingArea plantingArea)
    {
        Game.Logger.LogInfo("qwq", "placed");
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
        Destroyed?.Invoke();
        Board.PickingChanged -= EnterLightCheck;
    }

    public virtual void BeHurt(LawnEntity sourceEntity, double amount)
    {
        if (Game.OnPhysicsInterval(8))
            damagingParticleSys.Emit();
        Hp -= amount;
        HpChanged?.Invoke(-amount);
    }
}
