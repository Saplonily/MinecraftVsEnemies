namespace MVE;

public partial class RedstoneDisplayer : BoardUI
{
    protected Label amountLabel = null!;

    public Vector2 RedstoneOffset { get; set; }

    public override void _Ready()
    {
        base._Ready();
        RedstoneOffset = GetNode<Marker2D>("RedstoneMarker").GetPositionAndFree();
        amountLabel = GetNode<Label>("AmountLabel");

        Board.Bank.RedstoneChanged += _ =>
        {
            amountLabel.Text = Board.Bank.Redstone.ToString();
        };
    }
}
