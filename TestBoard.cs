namespace MVE;

public partial class TestBoard : Board
{
    public override void _Ready()
    {
        InitState = LevelState.Main; 
        List<EnemyPoolUnit> pool = new()
        {
            new() { Cost = 1, Weight = 0, Id = "normal" }
        };
        EnemiesSpawningData esd = new() { EnemyPool = pool };
        LevelData = new()
        {
            SceneId = "grasswalk",
            TotalWaves = 0,
            EnemiesSpawning = esd
        };
        base._Ready();
    }
}
