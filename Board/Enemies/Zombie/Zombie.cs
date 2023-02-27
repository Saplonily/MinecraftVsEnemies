using Godot;
using Saladim.GodotParticle;

namespace MVE;

public partial class Zombie : Enemy
{
    protected Area2D area2D = null!;
    protected AnimationTree animationTree = null!;
    protected AnimationNodeStateMachinePlayback mainAtSmPlayBack = null!;
    protected SalParticleSys deathParticleSys = null!;

    public Vector3 WalkingDirection = new(-1.0f, 0.0f, 0.0f);
    public float WalkingSpeed = 20.0f;
    public ZombieState State;
    public float Hp = 200.0f;
    public float MaxHp = 200.0f;

    public override void _Ready()
    {
        base._Ready();
        area2D = GetNode<Area2D>("Area2D");
        animationTree = GetNode<AnimationTree>("AnimationTree");
        mainAtSmPlayBack = animationTree.Get("parameters/MainStateMachine/playback").As<AnimationNodeStateMachinePlayback>();
        deathParticleSys = GetNode<SalParticleSys>("DeathParticle");
        RemoveChild(deathParticleSys);

        area2D.AreaShapeEntered += Area2D_AreaShapeEntered;
        animationTree.Active = true;
        State = ZombieState.Walking;

        mainAtSmPlayBack.Travel("Walking");

    }

    private void Area2D_AreaShapeEntered(Rid areaRid, Area2D area2d, long areaShapeIndex, long localShapeIndex)
    {
        if (State != ZombieState.Dying && area2d.Owner is Arrow arrow && !arrow.IsQueuedForDeletion())
        {
            arrow.QueueFree();
            Hp -= arrow.Damage;
            animationTree.Set("parameters/HitOneShot/request", 1);
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
        QueueFree();
        LeftParticle lp = new(deathParticleSys)
        {
            LevelPos = this.LevelPos
        };
        for(int i = 0; i < 100;  i++) 
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
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        Hp = Mathf.Clamp(Hp, 0.0f, MaxHp);
        if (Hp == 0.0f)
        {
            Die(DieReason.None);
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        switch (State)
        {
            case ZombieState.Walking:
            LevelPos += WalkingDirection * WalkingSpeed * (float)delta;
            break;
        }
    }
}

public enum ZombieState
{
    Idle,
    Walking,
    Dying
}

public enum DieReason
{
    None,
    Hit
}