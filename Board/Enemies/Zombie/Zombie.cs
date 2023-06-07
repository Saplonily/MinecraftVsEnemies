using MVE.SalExt;
using System.Diagnostics;

namespace MVE;

public record struct ZombieAttackInfo(Weapon? Attacking)
{
    public bool IsAttacking
    {
        readonly get => Attacking is not null;
        set
        {
            Debug.Assert(value == false);
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
    public override double Weight => 20d;

    public override void _Ready()
    {
        base._Ready();
        animationTree = GetNode<AnimationTree>("AnimationTree");
        mainAtSmPlayBack = animationTree.Get("parameters/MainStateMachine/playback").As<AnimationNodeStateMachinePlayback>();
        deathParticleSys = GetNode<SalParticleSys>("DeathParticle");
        beHitAudioPlayerChooser = SalAudioPool.GetChooserFromArray(beHitAudios, (new(default!, Bus: "Board")));
        deathAudioPlayer = SalAudioPool.GetPlayer(new(deathAudio, Bus: "Board"));

        animationTree.Active = true;

        WalkingSpeed = Board.Random.NextSingle(15f, 23f);

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
        if (Hp <= 0 && stateMachine.State != ZombieState.Dying)
            Die(DeathReason.None);
    }

    public void WalkingUpdate(double delta)
    {
        LevelPos += WalkingDirection * WalkingSpeed * (float)delta;
        var collidedWeapons = GetAttackableWeapons();
        Debug.Assert(!attackInfo.IsAttacking);

        if (collidedWeapons.Any())
        {
            attackInfo.Attacking = collidedWeapons.First();
            stateMachine.TravelTo(ZombieState.Attacking);
        }
    }

    public void AttackingUpdate(double delta)
    {
        Debug.Assert(attackInfo.IsAttacking);
        var toAttacks = GetAttackableWeapons();

        attackInfo.Attacking!.BeHurt(50 * delta);

        if (!toAttacks.Contains(attackInfo.Attacking))
        {
            if (!toAttacks.Any())
            {
                // 当前正在破坏的器械不在了并且没有其他碰撞到的, 回到默认状态
                attackInfo.IsAttacking = false;
                stateMachine.TravelTo(ZombieState.Walking);
            }
            else
            {
                // 否则破坏下一个器械
                attackInfo.Attacking = toAttacks.First();
            }
        }
    }

    protected IEnumerable<Weapon> GetCollidedWeapons()
        => hitBox.GetOverlappingAreas()
            .Where(a => a.Owner is Weapon wea && wea.IsCollidedInThicknessWith(this))
            .Select(a => (Weapon)a.Owner);

    protected IEnumerable<Weapon> GetAttackableWeapons()
        => GetCollidedWeapons().Where(w => w.Tag.Hasnt(WeaponTags.InGroundNotBeAttackable));

    public override void BeHit(Bullet sourceBullet)
    {
        if (sourceBullet is Arrow arrow)
        {
            arrow.Free();
            BeHurt(arrow.Damage);
        }
    }

    public override void BeHurt(double amount/*TODO: more arguments*/)
    {
        Hp -= amount;
        beHitAudioPlayerChooser.Choose().Play();
        animationTree.Set("parameters/HitOneShot/request", (long)AnimationNodeOneShot.OneShotRequest.Fire);
        if (Hp <= 0 && stateMachine.State != ZombieState.Dying)
            Die(DeathReason.None);
    }

    protected override void HitBox_AreaEntered(Area2D area)
    {
        if (stateMachine.State != ZombieState.Dying)
            base.HitBox_AreaEntered(area);
    }

    public override void Die(DeathReason reason)
    {
        base.Die(reason);
        stateMachine.State = ZombieState.Dying;
        mainAtSmPlayBack.Travel("Die");
        hitBox.SetCollisionLayerValue(2, false);
        hitBox.SetCollisionLayerValue(7, true);
        deathAudioPlayer.Play();
    }

    public void OnDieAnimationEnded()
    {
        RemoveChild(deathParticleSys);
        LeftParticle lp = new(deathParticleSys)
        {
            LevelPos = LevelPos + deathParticleSys.Position.ToVec3WithZ0()
        };
        for (int i = 0; i < 100; i++)
        {
            var r = Game.Instance.Random;
            Vector2 pos = new();
            pos.X = r.Next1m1Single(deathParticleSys.ParticlePositionRandomness.X);
            pos.Y = r.Next1m1Single(deathParticleSys.ParticlePositionRandomness.Y);
            pos.X = r.Next1m1Single(pos.X);
            pos.Y = r.Next1m1Single(pos.Y);
            pos += deathParticleSys.ParticlePosition;
            deathParticleSys.EmitAt(pos);
        }
        Lawn.AddChild(lp);
        DropLoot();
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
