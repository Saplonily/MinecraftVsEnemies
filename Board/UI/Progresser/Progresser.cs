using MVE.SalExt;

namespace MVE;

public partial class Progresser : BoardUI
{
    protected RemoteDrawer flagsDrawer = default!;
    protected Sprite2D fgSprite = default!;
    protected Rect2 fgSprRect;
    protected Tween? percentDisplayTween;
    protected float[] flagRisingPercents = default!;
    protected float flagRisingHeight = 100;

    [Export] public Texture2D WaveFlagTexture { get; protected set; } = default!;
    public float Percent { get; protected set; }
    public float DisplayPercent { get; set; }
    public float[] FlagPositions { get; set; } = default!;

    public override void _Ready()
    {
        base._Ready();
        flagsDrawer = GetNode<RemoteDrawer>("Flags");
        fgSprite = GetNode<Sprite2D>("Fg");

        fgSprRect = fgSprite.GetRect();
        flagsDrawer.AssignAction(DrawFlag);

        flagRisingHeight = fgSprRect.Size.Y / 2;

        // temp
        int totalWaves = Board.LevelData.TotalWaves;
        int flags = totalWaves / 10;
        FlagPositions = new float[flags];
        for (int i = 0; i < flags; i++)
        {
            FlagPositions[i] = 1.0f / totalWaves * ((i + 1) * 10);
        }
        // temp
        flagRisingPercents = new float[FlagPositions.Length];
        flagRisingPercents.Initialize();

        Board.WaveChanged += this.Board_WaveChanged;
        SetFgPercent(0f);
    }

    protected void Board_WaveChanged(Board board, int preWave, int curWave)
    {
        float p = curWave / (float)board.LevelData.TotalWaves;
        TweenFgPercent(p);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        Board.WaveChanged -= this.Board_WaveChanged;
    }

    public void DrawFlag(RemoteDrawer d)
    {
        float wFlag = WaveFlagTexture.GetSize().X;
        for (int i = 0; i < FlagPositions.Length; i++)
        {
            float xOffset = fgSprRect.Size.X * (1f - FlagPositions[i]);
            float x = xOffset - wFlag / 1.2f;
            float y = d.Position.Y - flagRisingHeight * flagRisingPercents[i];
            d.DrawTexture(WaveFlagTexture, new Vector2(x, y));
        }
    }

    public void TweenFgPercent(float percent)
    {
        percent = Math.Clamp(percent, 0f, 1f);
        for (int i = 0; i < FlagPositions.Length; i++)
        {
            if (percent >= FlagPositions[i] && flagRisingPercents[i] == 0f)
            {
                int curi = i;
                var risingTween = CreateTween()
                    .SetEase(Tween.EaseType.Out)
                    .SetTrans(Tween.TransitionType.Sine)
                    .TweenMethod(Callable.From<float>(f =>
                    {
                        flagRisingPercents[curi] = f;
                    }), 0f, 1f, 2d);
            }
        }
        percentDisplayTween?.Kill();
        percentDisplayTween = CreateTween()
            .SetEase(Tween.EaseType.Out)
            .SetTrans(Tween.TransitionType.Sine);
        percentDisplayTween.TweenMethod(Callable.From<float>(SetFgPercent), DisplayPercent, percent, 3.0d);
    }

    protected void SetFgPercent(float percent)
    {
        DisplayPercent = percent;
        float width = fgSprRect.Size.X;
        float pWidth = width * percent;
        float lpWidth = fgSprRect.Size.X - pWidth;
        fgSprite.Position = fgSprite.Position with { X = lpWidth };
        fgSprite.RegionRect = new(fgSprite.Position, fgSprRect.Size with { X = pWidth });
    }
}
