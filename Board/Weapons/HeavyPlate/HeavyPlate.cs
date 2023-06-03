namespace MVE;

public partial class HeavyPlate : Weapon
{
    public override void _Ready()
    {
        base._Ready();
        Tag.Add(WeaponTags.InGroundNotBeAttackable);
    }
}
