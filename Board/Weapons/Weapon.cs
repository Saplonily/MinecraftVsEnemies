namespace MVE;

public partial class Weapon : LawnEntity
{
    protected Area2D hitBoxArea = null!;

    public override void _Ready()
    {
        base._Ready();
        hitBoxArea = GetNode<Area2D>("HitArea");
    }

}