using System.Diagnostics;
using System.Reflection;

namespace MVE;

[DebuggerDisplay("{GetType().Name,nq}, {LevelPos}")]
public abstract partial class BoardEntity : Node2D
{
    [Export]
    public Vector3 LevelPos { get; set; } = Vector3.Zero;

    public Board Board { get; protected set; } = null!;

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