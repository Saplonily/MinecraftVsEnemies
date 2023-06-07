using System.Diagnostics;

namespace MVE;

/// <summary>
/// LawnEntity, 要求有ShadowSprite影子. 受重力、摩擦力等影响
/// </summary>
[DebuggerDisplay("{GetType().Name,nq}, {LevelPos}")]
public abstract partial class LawnEntity : BoardEntity
{
    protected Sprite2D shadowSprite = default!;

    protected Vector2 shadowSpriteOffset;

    public Vector3 Velocity;
    public Lawn Lawn { get; protected set; } = default!;
    public float Friction { get; set; } = 250f;
    public bool EnableGravity { get; protected set; } = true;
    public float Thickess { get; protected set; } = 35f;
    public bool IsOnGround { get; private set; }

    public override void _Ready()
    {
        base._Ready();
        Lawn = this.FindParent<Lawn>() ?? throw new NodeNotFoundException(nameof(MVE.Lawn));
        shadowSprite = GetNode<Sprite2D>("ShadowSprite") ?? throw new NodeNotFoundException(nameof(Sprite2D));
        shadowSprite.ZIndex = Board.ShadowZIndex;
        shadowSpriteOffset = shadowSprite.Position;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        shadowSprite.Position = shadowSpriteOffset + new Vector2(0, LevelPos.Z);
#if GAME_DEBUG
        if (Input.IsKeyPressed(Key.Q))
        {
            Velocity += new Vector3(0, 0, Board.Random.NextSingle(0, 15));
        }
        Modulate = IsOnGround ? Colors.White : Colors.Aqua;
#endif
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        float deltaf = (float)delta;
        bool onGround = false;
        Velocity = MathM.ApproachNoZ(Velocity, Vector3.Zero, Friction * deltaf);
        if (EnableGravity)
            Velocity += Board.Gravity * deltaf;
        LevelPos += Velocity * deltaf;
        if (LevelPos.Z < 0)
        {
            LevelPos.Z = 0;
            Velocity.Z = 0;
            onGround = true;
        }
        IsOnGround = onGround;
    }

    /// <summary>
    /// 是否在厚度上碰撞
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public bool IsCollidedInThicknessWith(LawnEntity other)
    {
        float thisY = LevelPos.Y;
        float otherY = other.LevelPos.Y;
        float thisY2 = LevelPos.Y + Thickess;
        float otherY2 = other.LevelPos.Y + other.Thickess;
        return thisY < otherY ? thisY2 >= otherY : thisY < otherY2;
    }
}
