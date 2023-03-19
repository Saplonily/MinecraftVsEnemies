namespace MVE;

public partial class Obsidian : Weapon
{
    protected Sprite2D breakStateSprite = default!;

    public override void _Ready()
    {
        base._Ready();

        breakStateSprite = GetNode<Sprite2D>("BreakState");

        Hp = 1500f;
        MaxHp = 1500f;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        breakStateSprite.Frame = (int)((1 - Hp / MaxHp) * (breakStateSprite.Hframes - 1));
    }
}
