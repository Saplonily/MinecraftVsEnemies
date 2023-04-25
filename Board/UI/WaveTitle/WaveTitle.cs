namespace MVE;

public partial class WaveTitle : Node2D
{
    public const float WidthLimit = 600f;
    protected Label label = default!;
    protected BoxContainer boxContainer = default!;
    protected AnimationPlayer animationPlayer = default!;

    public override void _Ready()
    {
        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        boxContainer = GetNode<BoxContainer>("TextContainer/BoxContainer");
        label = GetNode<Label>("TextContainer/BoxContainer/Label");

        if (label.Size.X > WidthLimit)
        {
            float radio = WidthLimit / label.Size.X;
            boxContainer.Scale = new Vector2(radio, radio);
        }
    }

    public async Task PlayAppear()
    {
        animationPlayer.Play("Appear");
        animationPlayer.Advance(0d);
        await ToSignal(animationPlayer, AnimationPlayer.SignalName.AnimationFinished);
    }
}
