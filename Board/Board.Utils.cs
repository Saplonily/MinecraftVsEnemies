namespace MVE;

public partial class Board
{
    public IEnumerable<Enemy> GetEnemies()
        => GetTree().GetNodesInGroup(GroupNames.Enemy).Where(n => n.FindParent<Board>() == this).Cast<Enemy>();
}
