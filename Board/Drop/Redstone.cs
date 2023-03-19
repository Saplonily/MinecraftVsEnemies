using MVE.SalExt;

namespace MVE;

public partial class Redstone : Drop
{
    [Export] protected AudioStream pickAudio = default!;
    protected AudioStreamPlayer pickAudioPlayer = default!;
    protected Sprite2D sprite = default!;

    public double Value { get => 25; }

    public override void _Ready()
    {
        base._Ready();
        sprite = GetNode<Sprite2D>("Redstone");
        pickAudioPlayer = SalAudioPool.GetPlayer(new(pickAudio, Bus: "Board"));
    }

    public override void OnPicking()
    {
        base.OnPicking();
        pickAudioPlayer.Play();
        shadowSprite.Visible = false;
        var tween =
             CreateTween()
            .SetEase(Tween.EaseType.Out)
            .SetTrans(Tween.TransitionType.Cubic);
        if (Board.RequestDropPicking(this) == DropPickResult.ToRedstoneDisplayer)
        {
            tween.TweenMethod(
                Callable.From<Vector3>(pos => LevelPos = pos),
                LevelPos,
                Board.GetRedstoneDisplayerSlotPos().ToVec3WithZ0(),
                0.5
                );
            tween.TweenCallback(Callable.From(() =>
            {
                Board.Bank.AddRedstone(Value);
                var tween = CreateTween()
                .SetEase(Tween.EaseType.Out)
                .SetTrans(Tween.TransitionType.Cubic);
                tween.TweenMethod(
                    Callable.From<Vector2>(v => sprite.Scale = v),
                    Vector2.One,
                    Vector2.Zero,
                    0.25
                    );
                tween.TweenCallback(Callable.From(QueueFree));
            }));
        }
    }
}
