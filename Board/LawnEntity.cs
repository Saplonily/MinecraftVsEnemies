using System.Diagnostics;

namespace MVE;

/// <summary>
/// LawnEntity, 要求有ShadowSprite影子. 受重力、摩擦力等影响
/// </summary>
[DebuggerDisplay("{GetType().Name,nq}, {LevelPos}")]
public abstract partial class LawnEntity : BoardEntity
{
    protected Sprite2D shadowSprite = null!;

    protected Vector2 shadowSpriteOffset;
    protected Vector3 velocity;

    public Lawn Lawn { get; protected set; } = null!;

    public ref Vector3 Velocity => ref velocity;

    public float Friction { get; set; } = 200f;

    public bool EnableGravity { get; protected set; } = true;

    public override void _Ready()
    {
        base._Ready();
        Lawn = this.FindParent<Lawn>() ?? throw new NodeNotFoundException(nameof(MVE.Lawn));
        shadowSprite = GetNode<Sprite2D>("ShadowSprite");
        shadowSpriteOffset = shadowSprite?.Position ?? Vector2.Zero;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        shadowSprite.Position = shadowSpriteOffset + new Vector2(0, LevelPos.Z);
        shadowSprite.ZIndex = Board.ShadowZIndex;
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        Velocity = Calculate.ApproachNonZ(Velocity, Vector3.Zero, Friction * (float)delta);
        if (EnableGravity)
            ApplyVelocity(Board.Gravity * (float)delta);
        LevelPos += Velocity * (float)delta;
        if (LevelPos.Z < 0)
        {
            LevelPos.Z = 0;
            Velocity.Z = 0;
        }
    }

    public void ApplyVelocity(Vector3 velocity)
        => Velocity += velocity;
}
