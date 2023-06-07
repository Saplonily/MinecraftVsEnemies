using System.Diagnostics;

namespace MVE;

[DebuggerDisplay("{GetType().Name,nq}, {LevelPos}")]
public abstract partial class BoardEntity : Node2D
{
    [Export] public Vector3 LevelPos;

    public Board Board { get; protected set; } = default!;

    public override void _Ready()
    {
        base._Ready();
        Position = new Vector2(LevelPos.X, LevelPos.Y - LevelPos.Z);
        Board = this.FindParent<Board>() ?? throw new NodeNotFoundException(nameof(MVE.Board));
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        Position = new Vector2(LevelPos.X, LevelPos.Y - LevelPos.Z);
    }
}
