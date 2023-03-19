using MVE.SalExt;
using Saladim.GodotParticle;

namespace MVE;

public partial class Dispenser : Weapon
{
    [Export]
    public static readonly PackedScene ArrowScene = GD.Load<PackedScene>("res://Board/Bullets/Arrow.tscn");

    [Export] protected AudioStream clickAudio = default!;
    protected AudioStreamPlayer clickAudioPlayer = default!;
    protected Timer shootTimer = default!;
    protected Vector2 shootOffset;
    protected RayCast2D rayCast = default!;
    protected SalParticleSys shootParticleSys = default!;

    public override void _Ready()
    {
        base._Ready();
        shootTimer = GetNode<Godot.Timer>("ShootTimer");
        shootOffset = GetNode<Marker2D>("ShootOffsetMarker").GetPositionAndFree();
        rayCast = GetNode<RayCast2D>("RayCast2D");
        shootParticleSys = GetNode<SalParticleSys>("ShootPtSys");
        clickAudioPlayer = SalAudioPool.GetPlayer(new(clickAudio, Bus: "Board"));

        shootTimer.Timeout += OnShootTimerTimeout;
        shootTimer.Start();
    }

    public void OnShootTimerTimeout()
    {
        GodotObject collider = rayCast.GetCollider();
        if (collider is Node node && node.Owner is BoardEntity levelEntity && levelEntity is Enemy)
        {
            Shoot();
            shootTimer.WaitTime = Game.Instance.Random.NextDouble(0.75, 1);
            shootTimer.Start();
            shootParticleSys.EmitMany(20);
        }
        else
        {
            shootTimer.WaitTime = Game.Instance.Random.NextDouble(0.5, 2);
            shootTimer.Start();
        }
    }

    public void Shoot()
    {
        clickAudioPlayer.Play();
        Lawn.AddBoardEntity(ArrowScene.Instantiate<Arrow>(), LevelPos + new Vector3(shootOffset.X, 0, -shootOffset.Y));
    }
}