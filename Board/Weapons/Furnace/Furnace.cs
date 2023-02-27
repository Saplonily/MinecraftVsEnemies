﻿using Saladim.GodotParticle;

namespace MVE;

public partial class Furnace : Weapon
{
    public static readonly PackedScene RedstoneScene = GD.Load<PackedScene>("res://Board/Drop/Redstone.tscn");

    protected SalParticleSys flameSalPSys = null!;
    protected SalParticleSys flame2SalPSys = null!;
    protected Godot.Timer flameTimer = null!;
    protected Godot.Timer produceTimer = null!;

    public override void _Ready()
    {
        base._Ready();
        flameSalPSys = GetNode<SalParticleSys>("FlameParticles");
        flame2SalPSys = GetNode<SalParticleSys>("Flame2Particles");
        flameTimer = GetNode<Godot.Timer>("FlameTimer");
        produceTimer = GetNode<Godot.Timer>("ProduceTimer");

        flameTimer.Timeout += EmitParticle;
        produceTimer.Timeout += Produce;

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
        Lawn.AddBoardEntity(redstone, LevelPos);
    }
}
