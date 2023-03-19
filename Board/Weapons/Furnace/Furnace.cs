using Saladim.GodotParticle;

namespace MVE;

public partial class Furnace : Weapon
{
    public static readonly PackedScene RedstoneScene = GD.Load<PackedScene>("res://Board/Drop/Redstone.tscn");

    protected SalParticleSys flameSalPSys = default!;
    protected SalParticleSys flame2SalPSys = default!;
    protected Godot.Timer flameTimer = default!;
    protected Godot.Timer produceTimer = default!;

    public override void _Ready()
    {
        base._Ready();
        flameSalPSys = GetNode<SalParticleSys>("FlameParticles");
        flame2SalPSys = GetNode<SalParticleSys>("Flame2Particles");
        flameTimer = GetNode<Godot.Timer>("FlameTimer");
        produceTimer = GetNode<Godot.Timer>("ProduceTimer");

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
        redstone.ApplyVelocity(new Vector3(Game.Instance.Random.NextFloat(-200, 200), 0, Game.Instance.Random.NextFloat(300)));
        Lawn.AddBoardEntity(redstone, LevelPos);
    }
}
