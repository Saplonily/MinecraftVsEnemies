namespace MVE;

public abstract partial class Enemy : LawnEntity
{
    protected bool enableHpLock = true;

    public double Hp { get; set; } = 200.0f;

    public double MaxHp { get; set; } = 200.0f;


    public override void _Process(double delta)
    {
        base._Process(delta);
        if (enableHpLock)
            Hp = Math.Clamp(Hp, 0d, MaxHp);
        if (Hp == 0d)
        {
            OnHpUseUp();
        }
    }

    public virtual void OnHpUseUp()
    {
    }
}