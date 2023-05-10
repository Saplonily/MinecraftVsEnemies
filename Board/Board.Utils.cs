namespace MVE;

public partial class Board
{
    public IEnumerable<Enemy> GetEnemies()
        => GetTree().GetNodesInGroup(GroupNames.Enemy).Cast<Enemy>().Where(n => n.Board == this);
}
