namespace MVE;

public partial class LeatZombie : Zombie
{
    protected Sprite2D leatCapSprite = null!;

    public double CapHp { get; set; } = 400d;

    public double CapMaxHp { get; set; } = 400d;

    public override void _Ready()
    {
        base._Ready();
        leatCapSprite = GetNode<Sprite2D>("Sprites/LeatCap");
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        CapHp = Math.Clamp(CapHp, 0d, CapMaxHp);
        leatCapSprite.Visible = CapHp > 0;
    }

    public override void BeHurt(double amount)
    {
        if (CapHp <= 0d)
        {
            base.BeHurt(amount);
        }
        else
        {
            CapHp -= amount;
            animationTree.Set("parameters/CapHitOneShot/request", (int)AnimationNodeOneShot.OneShotRequest.Fire);
        }
    }
}
