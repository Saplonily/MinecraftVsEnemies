using MVE.SalExt;

namespace MVE;

public partial class ReadySetUI : BoardUI
{
    [Export] protected AudioStream audio = default!;
    protected AnimationPlayer animationPlayer = default!;

    public override void _Ready()
    {
        base._Ready();
        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");

    }

    public async Task PlayAndFree()
    {
        var player = SalAudioPool.GetPlayer(new(audio, Bus: "UI"));
        player.Play();
        animationPlayer.Play("Main");
        await ToSignal(animationPlayer, AnimationPlayer.SignalName.AnimationFinished);
    }
}
