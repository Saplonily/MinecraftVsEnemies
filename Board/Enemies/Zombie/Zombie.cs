using System.Diagnostics;
using MVE.SalExt;

namespace MVE;

public record struct ZombieAttackInfo(Weapon? Attacking)
{
    public bool IsAttacking
    {
        get => Attacking is not null;
        set
        {
            Debug.Assert(!value);
            if (!value) Attacking = null;
        }
    }
}

public partial class Zombie : Enemy
{
    [Export] protected Godot.Collections.Array<AudioStream> beHitAudios = default!;
    protected Chooser<AudioStreamPlayer> beHitAudioPlayerChooser = default!;
    [Export] protected AudioStream deathAudio = default!;
    protected AudioStreamPlayer deathAudioPlayer = default!;
    protected AnimationTree animationTree = default!;
    protected AnimationNodeStateMachinePlayback mainAtSmPlayBack = default!;
    protected SalParticleSys deathParticleSys = default!;

    protected ZombieAttackInfo attackInfo;

    protected StateMachine<ZombieState> stateMachine = default!;

    public Vector3 WalkingDirection { get; protected set; } = new(-1.0f, 0.0f, 0.0f);
    public float WalkingSpeed { get; protected set; } = 20.0f;

    public override void _Ready()
    {
        base._Ready();
        animationTree = GetNode<AnimationTree>("AnimationTree");
        mainAtSmPlayBack = animationTree.Get("parameters/MainStateMachine/playback").As<AnimationNodeStateMachinePlayback>();
        deathParticleSys = GetNode<SalParticleSys>("DeathParticle");
        beHitAudioPlayerChooser = SalAudioPool.ChooserFromArray(beHitAudios, (new(default!, Bus: "Board")));
        deathAudioPlayer = SalAudioPool.GetPlayer(new(deathAudio, Bus: "Board"));

        RemoveChild(deathParticleSys);

        hitBox.AreaEntered += Area2D_AreaEntered;
        animationTree.Active = true;

        WalkingSpeed = Board.Random.NextFloat(15f, 23f);

        stateMachine = new();
        stateMachine.RegisterState(ZombieState.Idle);
        stateMachine.RegisterState(ZombieState.Walking, physicsUpdate: WalkingUpdate, enter: _ => mainAtSmPlayBack.Travel("Walking"));
        stateMachine.RegisterState(ZombieState.Attacking, physicsUpdate: AttackingUpdate, enter: _ => mainAtSmPlayBack.Travel("Attack"));
        stateMachine.RegisterState(ZombieState.Dying);
        stateMachine.State = ZombieState.Walking;
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        stateMachine.PhysicsUpdate(delta);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        stateMachine.Update(delta);
    }

    public override void OnHpUseUp()
    {
        if (stateMachine.State != ZombieState.Dying)
            Die(DieReason.None);
    }

    public void WalkingUpdate(double delta)
    {
        LevelPos += WalkingDirection * WalkingSpeed * (float)delta;
        var collidedWeapons = GetCollidedWeapons();
        Debug.Assert(!attackInfo.IsAttacking);

        if (collidedWeapons.Any())
        {
            attackInfo.Attacking = collidedWeapons.First();
            stateMachine.TravelTo(ZombieState.Attacking);
        }
    }

    public void AttackingUpdate(double delta)
    {
        var collidedWeapons = GetCollidedWeapons();
        Debug.Assert(attackInfo.IsAttacking);

        attackInfo.Attacking!.BeHurt(this, 50 * delta);

        if (!collidedWeapons.Contains(attackInfo.Attacking))
        {
            if (!collidedWeapons.Any())
            {
                attackInfo.IsAttacking = false;
                stateMachine.TravelTo(ZombieState.Walking);
            }
            else
            {
                attackInfo.Attacking = collidedWeapons.First();
            }
        }
    }

    protected IEnumerable<Weapon> GetCollidedWeapons()
        => hitBox.GetOverlappingAreas()
            .Where(a => a.Owner is Weapon wea && wea.IsCollidedInThicknessWith(this))
            .Select(a => (Weapon)a.Owner);

    public override void BeHit(Bullet sourceBullet)
    {
        if (sourceBullet is Arrow arrow)
        {
            arrow.Free();
            BeHurt(arrow.Damage);
        }
    }

    public override void BeHurt(double amount)
    {
        Hp -= amount;
        beHitAudioPlayerChooser.Choose().Play();
        animationTree.Set("parameters/HitOneShot/request", (int)AnimationNodeOneShot.OneShotRequest.Fire);
    }

    protected void Area2D_AreaEntered(Area2D area2d)
    {
        var nodeOwner = area2d.Owner;
        if (stateMachine.State != ZombieState.Dying)
        {
            if (nodeOwner is Bullet bullet)
            {
                BeHit(bullet);
            }
        }
    }

    public void Die(DieReason reason)
    {
        stateMachine.State = ZombieState.Dying;
        mainAtSmPlayBack.Travel("Die");
        hitBox.CollisionLayer &= 1 << 0;
        hitBox.CollisionLayer |= 1 << 5;
        deathAudioPlayer.Play();
    }

    public void OnDieAnimationEnded()
    {
        LeftParticle lp = new(deathParticleSys)
        {
            LevelPos = LevelPos + deathParticleSys.Position.ToVec3WithZ0()
        };
        for (int i = 0; i < 100; i++)
        {
            var r = Game.Instance.Random;
            Vector2 pos = new();
            pos.X = r.Next1m1Float(deathParticleSys.ParticlePositionRandomness.X);
            pos.Y = r.Next1m1Float(deathParticleSys.ParticlePositionRandomness.Y);
            pos.X = r.Next1m1Float(pos.X);
            pos.Y = r.Next1m1Float(pos.Y);
            pos += deathParticleSys.ParticlePosition;
            deathParticleSys.EmitAt(pos);
        }
        Lawn.AddChild(lp);
        OnDead();
        QueueFree();
    }
}

public enum ZombieState
{
    Idle,
    Walking,
    Dying,
    Attacking
}

public enum DieReason
{
    None,
    Hit
}
