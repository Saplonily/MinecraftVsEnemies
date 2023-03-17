using System.Collections.Immutable;
using MVE.SalExt;

namespace MVE;

public partial class Board : Node
{
    [Export] protected AudioStream awoogaAudio = null!;
    protected AudioStreamPlayer awoogaAudioPlayer = null!;
    protected Timer spawnerTimer = null!;
    

    protected int[] rowWeights = new int[5];

    public int CurrentWave { get; protected set; }
    public LevelData LevelData { get; set; } = null!;
    public bool SpawnerBeginningReadyed { get; protected set; } = false;

    public void InitSpawner()
    {
        spawnerTimer = GetNode<Timer>("SpawnerTimer");
        awoogaAudioPlayer = SalAudioPool.GetPlayer(new(awoogaAudio, Bus: "Board"));

        spawnerTimer.WaitTime = 50;
        spawnerTimer.Timeout += SpawnerTimerTimeout;

        CurrentWave = 0;
    }

    public void SpawnerTimerTimeout()
    {
        if (!SpawnerBeginningReadyed)
        {
            SpawnerBeginningReadyed = true;
            awoogaAudioPlayer.Play();
        }
        spawnerTimer.WaitTime = Random.NextDouble(10, 14);
        spawnerTimer.Start();
        NextWave();
    }

    public void UpdateSpawner(double delta)
    {
        var count = GetTree().GetNodesInGroup("Enemy").Count;
        if (count == 0 && SpawnerBeginningReadyed)
        {
            if (spawnerTimer.TimeLeft >= 0.75d)
            {
                spawnerTimer.WaitTime = 0.75d;
                spawnerTimer.Start();
            }
        }
    }

    public void NextWave()
    {
        CurrentWave += 1;

        int points = CurrentWave * LevelData.EnemiesSpawning.PointsAddFactor;

        SummonEnemiesBy(LevelData.EnemiesSpawning, points, PlaceEnemy);
    }

    public delegate Enemy EnemyPlacingHandler(in EnemyProperty property, int row);

    public void SummonEnemiesBy(EnemiesSpawningData spawningData, int points, EnemyPlacingHandler placingHandler)
    {
        var leastCost = spawningData.EnemyPool.Min(u => u.Cost);
        if (leastCost > points)
        {
            int row = Random.Next(0, 5);
            var prop = Game.Instance.EnemyProperties[0];
            placingHandler(prop, row);
            var u = spawningData.EnemyPool.FirstOrDefault(u => u.InternalId == 0);
            rowWeights[row] -= u is not null ? u.Cost : 100;
            return;
        }

        while (points > 0)
        {
            var unit = spawningData.ChooseUnit(Random, points);
            if (unit is null) break;
            points -= unit.Cost;

            var curMax = rowWeights.Max();
            var allMax = rowWeights.Select((i, ind) => (i, ind)).Where(i => i.i == curMax);
            var allMaxList = allMax.ToList();
            (int i, int ind) result = Chooser<(int, int)>.ChooseFrom(Random, allMaxList);
            rowWeights[result.ind] -= unit.Cost;

            placingHandler(Game.Instance.EnemyProperties[unit.InternalId], result.ind);
        }
    }

    public Enemy PlaceEnemy(in EnemyProperty property, int row)
        => Lawn.PlantingArea.PlaceEnemyAt(new Vector2I(Random.Next(10, 13), row), property);
}
