using System.Diagnostics;

namespace MVE;

[DebuggerDisplay("{GetType().Name,nq}, {LevelPos}")]
public abstract partial class LawnEntity : BoardEntity
{
    protected Vector2 shadowSpriteOffset;

    public Sprite2D ShadowSprite { get; protected set; } = null!;

    public Lawn Lawn { get; protected set; } = null!;

    public override void _Ready()
    {
        base._Ready();
        Lawn = this.FindParent<Lawn>() ?? throw new NodeNotFoundException(nameof(MVE.Lawn));
        ShadowSprite = GetNode<Sprite2D>("ShadowSprite");
        shadowSpriteOffset = ShadowSprite?.Position ?? Vector2.Zero;
        
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        ShadowSprite.Position = shadowSpriteOffset + new Vector2(0, LevelPos.Z);
        ShadowSprite.ZIndex = Board.ShadowZIndex;
    }
}
