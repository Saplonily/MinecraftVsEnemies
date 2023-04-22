using System.Diagnostics;
using System.Text.Json.Serialization;

namespace MVE;

[DebuggerDisplay("Scene: {SceneId,nq},TotalWaves: {TotalWaves}")]
public class LevelData
{
    public required Sid SceneId { get; set; }
    public required int TotalWaves { get; set; }
    public required EnemiesSpawningData EnemiesSpawning { get; set; }
    public double WaveTimerBase { get; set; } = 15f;
    public double WaveTimerTemperature { get; set; } = 5f;
    public LevelInventory Inventory { get; set; } = new();
    public WaveEventData? WaveEvent { get; set; }
    public Dictionary<string, string> Meta { get; set; } = new();
}

[DebuggerDisplay("EnemyPool, Count={EnemyPool.Count}")]
public class EnemiesSpawningData
{
    public required List<EnemyPoolUnit> EnemyPool { get; set; }
    public int PointsAddFactor { get; set; } = 50;

    /// <summary>
    /// 从该出怪配置中使用最大花费选出一只Enemy, 返回null时表示当前总花费不可寻找任何一只Enemy
    /// </summary>
    /// <param name="random">随机数提供器</param>
    /// <param name="maxCost">总花费</param>
    /// <returns>结果</returns>
    /// <exception cref="Exception">当内部加权随机出错时抛出, 该异常抛出是应该被避免的</exception>
    public EnemyPoolUnit? ChooseUnit(Random random, int maxCost)
    {
        var costablePool = EnemyPool.Where(u => u.Cost <= maxCost);
        if (!costablePool.Any())
            return null;
        // 1919810
        EnemyPoolUnit? unitSelected =
            Calculate.ChooseByWeight(random, costablePool, costablePool.Select(p => p.Weight))
            ?? throw new Exception("No unit found.");
        return unitSelected;
    }
}

[DebuggerDisplay("Id: {Id}, Weight: {Weight}, Cost: {Cost}")]
public class EnemyPoolUnit
{
    public required Sid Id { get; set; }
    public required int Weight { get; set; }
    public required int Cost { get; set; }
}

public class LevelInventory
{
    public List<Sid> CardsAlwaysIncludes { get; set; } = new();
}

public class WaveEventData
{
    public Dictionary<string, WaveEvent> EventStores { get; set; } = new();

    public Dictionary<string, List<WaveEvent>> Events { get; set; } = new();
}
