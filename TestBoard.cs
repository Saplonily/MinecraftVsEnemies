﻿namespace MVE;

public partial class TestBoard : Board
{
    public override void _Ready()
    {
        InitState = LevelState.Main;
        List<EnemyPoolUnit> pool = new()
        {
            new() { Cost = 100, Weight = 1, Id = "zombie" },
            new() { Cost = 100, Weight = 1, Id = "leat_zombie" },
            new() { Cost = 100, Weight = 1, Id = "iron_zombie" }
        };
        EnemiesSpawningData esd = new() { EnemyPool = pool, PointsAddFactor = 100 };
        LevelData = new()
        {
            SceneId = "grasswalk",
            TotalWaves = 0,
            EnemiesSpawning = esd
        };
        base._Ready();
    }
}
