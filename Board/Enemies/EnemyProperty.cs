namespace MVE;

public partial class EnemyProperty
{
    public PackedScene Scene;

    public float Hp;

    public float MaxHp;

    public EnemyProperty(PackedScene scene, float hp, float maxHp)
    {
        Scene = scene;
        Hp = hp;
        MaxHp = maxHp;
    }
}