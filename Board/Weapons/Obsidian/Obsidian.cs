namespace MVE;

public partial class Obsidian : Weapon
{
    protected Sprite2D breakStateSprite = null!;

    public override void _Ready()
    {
        base._Ready();

        breakStateSprite = GetNode<Sprite2D>("BreakState");

        Hp = 3000f;
        MaxHp = 3000f;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        breakStateSprite.Frame = (int)((1 - Hp / MaxHp) * breakStateSprite.Hframes);
    }
}
