namespace MVE;

public partial class BoardUI : Node2D
{
    public Board Board { get; protected set; } = null!;

    public override void _Ready()
    {
        Board = this.FindParent<Board>() ?? throw new NodeNotFoundException(nameof(MVE.Board));
    }
}
