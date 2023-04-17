namespace MVE;

public partial class RedstoneDisplayer : BoardUI
{
    protected Label amountLabel = default!;

    public Vector2 RedstoneOffset { get; set; }

    public override void _Ready()
    {
        base._Ready();
        RedstoneOffset = GetNode<Marker2D>("RedstoneMarker").GetPositionAndFree();
        amountLabel = GetNode<Label>("AmountLabel");

        Board.Bank.RedstoneChanged += BoardBank_RedstoneChanged;
    }

    protected void BoardBank_RedstoneChanged(double delta)
    {
        amountLabel.Text = Board.Bank.Redstone.ToString();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        Board.Bank.RedstoneChanged -= BoardBank_RedstoneChanged;
    }
}
