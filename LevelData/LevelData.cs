using System.Diagnostics;

namespace MVE;

[DebuggerDisplay("TotalWaves: {TotalWaves}")]
public class LevelData
{
    public required int TotalWaves { get; init; }

    public required EnemiesSpawningData EnemiesSpawning { get; init; }
    public required WaveEventsData WaveEvents { get; init; }
    public required Dictionary<string, string> Meta { get; init; }
}

[DebuggerDisplay("EnemyPool, Count={EnemyPool.Count}")]
public class EnemiesSpawningData
{
    public required List<EnemyPoolUnit> EnemyPool { get; init; }
    public required int PointsAddFactor { get; init; }

    public EnemyPoolUnit? ChooseUnit(Random random, int maxCost)
    {
        var costablePool = EnemyPool.Where(u => u.Cost <= maxCost);
        if (!costablePool.Any()) return null;

        int totalWeight = costablePool.Sum(u => u.Weight);
        int targetWeight = random.Next(totalWeight);
        int currentWeight = 0;
        EnemyPoolUnit? unitSelected = null;
        foreach (var unit in costablePool)
        {
            currentWeight += unit.Weight;
            if (currentWeight >= targetWeight)
            {
                unitSelected = unit;
                break;
            }
        }
        if (unitSelected is null)
            throw new InvalidOperationException("No unit found.");
        return unitSelected;
    }
}

[DebuggerDisplay("Marker: {Marker,nq}, Id: {InternalId}, Weight: {Weight}, Cost: {Cost}")]
public class EnemyPoolUnit
{
    public required string Marker { get; init; }
    public required int InternalId { get; init; }
    public required int Weight { get; init; }
    public required int Cost { get; init; }
}

public record WaveEventsData();