namespace MVE;

public partial class MainBoardUIManager : Node2D
{
    protected AnimationPlayer animationPlayer = default!;

    public RedstoneDisplayer RedstoneDisplayer { get; set; } = default!;

    [Export] public Vector2 CardsLayoutStartPos { get; protected set; }

    public override void _Ready()
    {
        RedstoneDisplayer = GetNode<RedstoneDisplayer>("RedstoneDisplayer");
        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
    }

    public async Task PlayDisplayAnimation()
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

    public void RequestAllDisabledChange(bool toDisabled)
    {
        foreach (var u in this.GetChildren().OfType<BoardUI>())
        {
            u.Disabled = toDisabled;
        }
    }
}
