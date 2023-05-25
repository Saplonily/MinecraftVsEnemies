namespace MVE;

public partial class MainBoardUIManager : Node2D
{
    [Export] protected AnimationPlayer animationPlayer = default!;
    [Export] public Progresser Progresser { get; set; } = default!;
    [Export] public RedstoneDisplayer RedstoneDisplayer { get; set; } = default!;

    [Export] public Vector2 CardsLayoutStartPos { get; protected set; }


    public async Task DisplayMain()
    {
        animationPlayer.Play("MainDisplay");
        animationPlayer.Advance(0);
        await ToSignal(animationPlayer, AnimationPlayer.SignalName.AnimationFinished);
    }

    public void MakeMainHide()
    {
        animationPlayer.Play("MainHide");
        animationPlayer.Advance(0);
    }

    public async Task DisplayProgresser()
    {
        animationPlayer.Play("ProgresserDisplay");
        animationPlayer.Advance(0);
        await ToSignal(animationPlayer, AnimationPlayer.SignalName.AnimationFinished);
    }

    public void RequestAllDisabledChange(bool toDisabled)
    {
        foreach (var u in this.GetChildren().OfType<BoardUI>())
        {
            u.Disabled = toDisabled;
        }
    }
}
