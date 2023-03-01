namespace MVE;

public abstract partial class Enemy : LawnEntity
{
    protected double hp = 200.0f;
    protected double maxHp = 200.0f;
    protected bool enableHpLock = true;

    public override void _Process(double delta)
    {
        base._Process(delta);
        if (enableHpLock)
            hp = Math.Clamp(hp, 0d, maxHp);
        if (hp == 0d)
        {
            OnHpUseUp();
        }
    }

    public virtual void OnHpUseUp()
    {
    }
}