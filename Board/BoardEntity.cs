using System.Diagnostics;
using System.Reflection;

namespace MVE;

[DebuggerDisplay("{GetType().Name,nq}, {LevelPos}")]
public abstract partial class BoardEntity : Node2D
{
    [Export]
    protected Vector3 levelPos;

    public ref Vector3 LevelPos => ref levelPos;

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