namespace MVE;

public partial class Board
{
    public IEnumerable<T> TrackerGet<T>(StringName name) where T : BoardEntity
        => GetTree().GetNodesInGroup(name).Cast<T>().Where(n => n.Board == this && !n.IsQueuedForDeletion());

    public IEnumerable<Enemy> GetEnemies()
        => TrackerGet<Enemy>(GroupNames.Enemy);
}
