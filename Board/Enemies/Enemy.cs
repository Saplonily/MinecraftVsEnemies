namespace MVE;

public abstract partial class Enemy : LawnEntity
{
    protected Area2D hitBox = default!;
    protected bool enableHpLock = true;

    public event Action<Enemy, DeathReason>? Died;
    public double Hp { get; set; } = 200.0f;
    public double MaxHp { get; set; } = 200.0f;
    public abstract double Weight { get; }

    public override void _Ready()
    {
        base._Ready();
        hitBox = GetNode<Area2D>("HitBox");
        hitBox.AreaEntered += HitBox_AreaEntered;
    }

    protected virtual void HitBox_AreaEntered(Area2D area)
    {
        var nodeOwner = area.Owner;
        if (nodeOwner is Bullet bullet && bullet.IsCollidedInThicknessWith(this))
        {
            BeHit(bullet);
        }
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        if (enableHpLock)
            Hp = Math.Clamp(Hp, 0d, MaxHp);
    }

    public virtual void DropLoot()
    {
        if (Board.IsFinalWave && Board.GetEnemies().Only(this))
            Board.DropFinalAward(LevelPos);
    }

    public virtual void BeHit(Bullet bullet)
    {
    }

    public virtual void BeHurt(double amount)
    {
    }

    public virtual void Die(DeathReason dieReason)
    {
        Died?.Invoke(this, dieReason);
    }
}
