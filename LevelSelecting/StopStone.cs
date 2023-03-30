namespace MVE.LevelSelecting;

public partial class StopStone : Node2D
{
    [Export] public Direction DirectionAllowed { get; set; }
    public virtual bool AbleToEnter => false;

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
