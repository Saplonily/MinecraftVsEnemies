using System.Diagnostics;
using Godot;
using Saladim.GodotParticle;

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
    protected Area2D area2D = null!;
    protected AnimationTree animationTree = null!;
    protected AnimationNodeStateMachinePlayback mainAtSmPlayBack = null!;
    protected SalParticleSys deathParticleSys = null!;

    protected ZombieAttackInfo attackInfo;

    public Vector3 WalkingDirection { get; protected set; } = new(-1.0f, 0.0f, 0.0f);
    public float WalkingSpeed { get; protected set; } = 20.0f;
    public ZombieState State { get; protected set; }

    public override void _Ready()
    {
        base._Ready();
        area2D = GetNode<Area2D>("Area2D");
        animationTree = GetNode<AnimationTree>("AnimationTree");
        mainAtSmPlayBack = animationTree.Get("parameters/MainStateMachine/playback").As<AnimationNodeStateMachinePlayback>();
        deathParticleSys = GetNode<SalParticleSys>("DeathParticle");
        RemoveChild(deathParticleSys);

        area2D.AreaEntered += Area2D_AreaEntered;
        animationTree.Active = true;
        RequestStateChange(ZombieState.Walking);
    }

    public override void OnHpUseUp()
    {
        if (State != ZombieState.Dying)
            Die(DieReason.None);
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        switch (State)
        {
            case ZombieState.Walking:
            {
                LevelPos += WalkingDirection * WalkingSpeed * (float)delta;
                var collidedWeapons = GetCollidedWeapons();
                Debug.Assert(!attackInfo.IsAttacking);

                if (collidedWeapons.Any())
                {
                    attackInfo.Attacking = collidedWeapons.First();
                    RequestStateChange(ZombieState.Attacking);
                }
            }
            break;
            case ZombieState.Attacking:
            {
                var collidedWeapons = GetCollidedWeapons();
                Debug.Assert(attackInfo.IsAttacking);

                attackInfo.Attacking!.BeHurt(this, 50 * delta);

                if (!collidedWeapons.Contains(attackInfo.Attacking))
                {
                    if (!collidedWeapons.Any())
                    {
                        attackInfo.IsAttacking = false;
                        RequestStateChange(ZombieState.Walking);
                    }
                    else
                    {
                        attackInfo.Attacking = collidedWeapons.First();
                    }
                }
            }
            break;
        }
        IEnumerable<Weapon> GetCollidedWeapons()
            => area2D.GetOverlappingAreas()
                .Where(a => a.Owner is Weapon wea && wea.IsCollidedInThicknessWith(this))
                .Select(a => (Weapon)a.Owner);
    }

    public void RequestStateChange(ZombieState target)
    {
        var preState = State;
        var targetState = target;
        State = targetState;
        if (target is ZombieState.Walking)
        {
            mainAtSmPlayBack.Travel("Walking");
        }
        else if (target is ZombieState.Attacking)
        {
            mainAtSmPlayBack.Travel("Attack");
        }
    }

    public void BeHit(Bullet sourceBullet)
    {
        if (sourceBullet is Arrow arrow)
        {
            arrow.QueueFree();
            BeHurt(arrow.Damage);
        }
    }

    public void BeHurt(double amount)
    {
        hp -= amount;
        animationTree.Set("parameters/HitOneShot/request", 1);
    }

    protected void Area2D_AreaEntered(Area2D area2d)
    {
        var nodeOwner = area2d.Owner;
        if (State != ZombieState.Dying)
        {
            if (nodeOwner is Bullet bullet)
            {
                BeHit(bullet);
            }
        }
    }

    public void Die(DieReason reason)
    {
        State = ZombieState.Dying;
        mainAtSmPlayBack.Travel("Die");
        area2D.CollisionLayer &= 1 << 0;
        area2D.CollisionLayer |= 1 << 5;
    }

    public void OnDieAnimationEnded()
    {
        LeftParticle lp = new(deathParticleSys);
        lp.LevelPos = LevelPos + deathParticleSys.Position.ToVec3WithZ0();
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