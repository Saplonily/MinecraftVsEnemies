using MVE.SalExt;

namespace MVE;

public partial class Progresser : BoardUI
{
    protected float percent = 0.0f;
    protected RemoteDrawer flagsDrawer = default!;
    protected Sprite2D fgSprite = default!;
    protected Rect2 fgSprRect;

    [Export] public Texture2D WaveFlagTexture { get; protected set; } = default!;
    public float Percent { get => percent; set { UpdateFgPercent(value); percent = value; } }
    public int Flags { get; set; }

    public override void _Ready()
    {
        base._Ready();
        flagsDrawer = GetNode<RemoteDrawer>("Flags");
        fgSprite = GetNode<Sprite2D>("Fg");

        fgSprRect = fgSprite.GetRect();
        flagsDrawer.AssignAction(DrawFlag);
    }

    public void DrawFlag(RemoteDrawer d)
    {
        d.DrawTexture(WaveFlagTexture, d.Position);
    }

    public void UpdateFgPercent(float percent)
    {
        float width = fgSprRect.Size.X;
        float pWidth = width * percent;
        float lpWidth = fgSprRect.Size.X - pWidth;
        fgSprite.Position = fgSprite.Position with { X = lpWidth };
        fgSprite.RegionRect = new(fgSprite.Position, fgSprRect.Size with { X = pWidth });
    }
}
