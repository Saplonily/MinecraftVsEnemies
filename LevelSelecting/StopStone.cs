using System.Diagnostics;

namespace MVE.LevelSelecting;

public partial class StopStone : Node2D
{
    protected Area2D area = default!;
    [Export] public Direction DirectionAllowed { get; set; }
    public virtual bool AbleToEnter => false;

    public override void _Ready()
    {
        area = GetNode<Area2D>("Area2D");

        area.InputEvent += this.Area_InputEvent;
    }

    private void Area_InputEvent(Node viewport, InputEvent @event, long shapeIdx)
    {
        if (AbleToEnter && @event is InputEventScreenTouch)
        {
            this.OnEnter(null!);
        }
    }

    public virtual void OnEnter(PlayerHead playerHead)
    {
        Debug.Fail("OnEnter called on StopStone");
    }

    [Flags]
    public enum Direction
    {
        None = 0,
        Left = 1 << 1,
        Right = 1 << 2,
        Up = 1 << 3,
        Down = 1 << 4
    }

    public static bool TryGetDirection(Vector2I dirVec, out Direction dir)
    {
        dir = Direction.None;
        if (dirVec.X == 1 && dirVec.Y == 0)
            dir = Direction.Right;
        else if (dirVec.X == -1 && dirVec.Y == 0)
            dir = Direction.Left;
        else if (dirVec.X == 0 && dirVec.Y == 1)
            dir = Direction.Down;
        else if (dirVec.X == 0 && dirVec.Y == -1)
            dir = Direction.Up;
        else
            return false;

        return true;
    }
}
