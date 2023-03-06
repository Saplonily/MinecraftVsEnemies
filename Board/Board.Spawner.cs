namespace MVE;

public partial class Board : Node
{
    protected Timer spawnerTimer = null!;
    protected Label debugLabel = null!;

    public int CurrentWave { get; protected set; }

    public LevelData LevelData { get; set; } = null!;

    public void InitSpawner()
    {
        spawnerTimer = GetNode<Timer>("SpawnerTimer");
        debugLabel = GetNode<Label>("LayerPicking/Label");

        spawnerTimer.Timeout += NextWave;

        CurrentWave = 0;
    }

    public void NextWave()
    {
        CurrentWave += 1;

        int points = CurrentWave * LevelData.EnemiesSpawning.PointsAddFactor;

        var leastCost = LevelData.EnemiesSpawning.EnemyPool.Min(u => u.Cost);
        if (leastCost > points)
        {
            PlaceEnemy(Game.Instance.EnemyProperties[0]);
            return;
        }

        while (points > 0)
        {
            var unit = LevelData.EnemiesSpawning.ChooseUnit(Random, points);
            points -= unit.Cost;
            PlaceEnemy(Game.Instance.EnemyProperties[unit.InternalId]);
        }

        void PlaceEnemy(in EnemyProperty property)
        {
            Lawn.PlantingArea.PlaceEnemyAt(new Vector2I(10, Random.Next(0, 5)), property);
        }
    }
}
