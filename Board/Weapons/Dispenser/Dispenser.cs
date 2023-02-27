using Saladim.GodotParticle;

namespace MVE;

public partial class Dispenser : Weapon
{
    public static readonly PackedScene ArrowScene = GD.Load<PackedScene>("res://Board/Bullets/Arrow.tscn");

    protected Godot.Timer shootTimer = null!;
    protected Marker2D shootOffset = null!;
    protected RayCast2D rayCast = null!;
    protected SalParticleSys shootParticleSys = null!;

    public override void _Ready()
    {
        base._Ready();
        shootTimer = GetNode<Godot.Timer>("ShootTimer");
        shootOffset = GetNode<Marker2D>("ShootOffsetPosition");
        rayCast = GetNode<RayCast2D>("RayCast2D");
        shootParticleSys = GetNode<SalParticleSys>("ShootPtSys");

        shootTimer.Timeout += OnShootTimerTimeout;
        shootTimer.Start();
    }

    public void OnShootTimerTimeout()
    {
        shootTimer.WaitTime = Game.Instance.Random.NextDouble(0.5, 3);
        GodotObject collider = rayCast.GetCollider();
        if (collider is Node node && node.Owner is BoardEntity levelEntity && levelEntity is Enemy)
        {
            Shoot();
            shootTimer.WaitTime = Game.Instance.Random.NextDouble(0.75, 1);
            shootParticleSys.EmitMany(20);
        }
    }

    public void Shoot()
    {
        Lawn.AddBoardEntity(ArrowScene.Instantiate<Arrow>(), LevelPos + new Vector3(shootOffset.Position.X, 0, -shootOffset.Position.Y));
    }
}