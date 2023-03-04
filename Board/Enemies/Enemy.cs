namespace MVE;

public abstract partial class Enemy : LawnEntity
{
    protected Area2D hitBox = null!;
    protected bool enableHpLock = true;

    public double Hp { get; set; } = 200.0f;

    public double MaxHp { get; set; } = 200.0f;


    public override void _Ready()
    {
        base._Ready();
        hitBox = GetNode<Area2D>("HitBox");

    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        if (enableHpLock)
            Hp = Math.Clamp(Hp, 0d, MaxHp);
        if (Hp <= 0d)
        {
            OnHpUseUp();
        }
    }

    public virtual void OnHpUseUp()
    {
    }

    public virtual void BeHit(Bullet bullet)
    {
    }

    public virtual void BeHurt(double amount)
    {
    }
}