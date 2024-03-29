using MVE.SalExt;

namespace MVE;

public partial class Redstone : Drop
{
    [Export] protected AudioStream pickAudio = default!;
    protected AudioStreamPlayer pickAudioPlayer = default!;
    protected Sprite2D sprite = default!;
    protected Tween? disappearTween;
    protected SceneTreeTimer disappearTimer = default!;

    public double Value { get => 25; }

    public override void _Ready()
    {
        base._Ready();
        sprite = GetNode<Sprite2D>("Redstone");
        pickAudioPlayer = SalAudioPool.GetPlayer(new(pickAudio, Bus: "Board"));
        disappearTimer = GetTree().CreateTimer(10);
        disappearTimer.Timeout += OnDisappearing;
    }

    public void OnDisappearing()
    {
        disappearTween = CreateTween().SetTrans(Tween.TransitionType.Expo).SetEase(Tween.EaseType.Out);
        disappearTween.TweenProperty(this, "modulate", Colors.Transparent, 0.2d);
        disappearTween.TweenCallback(Callable.From(Free));
    }

    public override bool OnHover()
        => disappearTween == null;

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
