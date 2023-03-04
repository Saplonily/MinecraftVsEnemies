namespace MVE;

public partial class IronZombie : Zombie
{
    protected Sprite2D ironCapSprite = null!;

    public double CapHp { get; set; } = 1000d;

    public double CapMaxHp { get; set; } = 1000d;

    public override void _Ready()
    {
        base._Ready();
        ironCapSprite = GetNode<Sprite2D>("Sprites/IronCap");
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        CapHp = Math.Clamp(CapHp, 0d, CapMaxHp);
        ironCapSprite.Visible = CapHp > 0;
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
