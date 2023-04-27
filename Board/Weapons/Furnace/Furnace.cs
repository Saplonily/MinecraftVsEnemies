using MVE.SalExt;

namespace MVE;

public partial class Furnace : Weapon
{
    public static readonly PackedScene RedstoneScene = GD.Load<PackedScene>("res://Board/Drop/Redstone.tscn");

    protected SalParticleSys flameSalPSys = default!;
    protected SalParticleSys flame2SalPSys = default!;
    protected Timer flameTimer = default!;
    protected Timer produceTimer = default!;

    public override void _Ready()
    {
        base._Ready();
        flameSalPSys = GetNode<SalParticleSys>("FlameParticles");
        flame2SalPSys = GetNode<SalParticleSys>("Flame2Particles");
        flameTimer = GetNode<Timer>("FlameTimer");
        produceTimer = GetNode<Timer>("ProduceTimer");

        flameTimer.Timeout += EmitParticle;
        produceTimer.Timeout += () =>
        {
            flameSalPSys.EmitMany(30);
            Produce();
            produceTimer.WaitTime = Game.Instance.Random.NextDouble(14d, 16d);
        };

        EmitParticle();
        void EmitParticle()
        {
            flameSalPSys.EmitMany(3);
            flame2SalPSys.Emit();
            flameTimer.WaitTime = Game.Instance.Random.NextDouble(4d, 6d);
        }
    }

    public void Produce()
    {
        var redstone = RedstoneScene.Instantiate<Redstone>();
        float velocity = Board.Random.NextFloat(200, 400);
        float dir =
            Board.Random.Next1m1() is 1 ?
            Board.Random.NextFloat(MathF.PI / 8 * 2, MathF.PI / 8 * 3) :
            Board.Random.NextFloat(MathF.PI / 8 * 5, MathF.PI / 8 * 6);
        Vector2 vec2 = Vector2.FromAngle(-dir) * velocity;
        Vector3 result = new(vec2.X, 0f, -vec2.Y);
        redstone.ApplyVelocity(result);
        Lawn.AddBoardEntity(redstone, LevelPos);
    }
}
