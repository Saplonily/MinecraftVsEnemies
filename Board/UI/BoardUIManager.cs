namespace MVE;

public partial class BoardUIManager : Node2D
{
    protected AnimationPlayer animationPlayer = default!;

    public RedstoneDisplayer RedstoneDisplayer { get; set; } = default!;

    [Export] public Vector2 CardsLayoutStartPos { get; protected set; }

    public override void _Ready()
    {
        RedstoneDisplayer = GetNode<RedstoneDisplayer>("RedstoneDisplayer");
        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
    }

    public void PlayDisplayAnimation()
    {
        animationPlayer.Play("Display");
    }

    public void PlayHideAnimation()
    {
        animationPlayer.Play("Hide");
    }
}