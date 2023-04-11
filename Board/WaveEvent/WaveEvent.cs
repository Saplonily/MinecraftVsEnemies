using System.Text.Json.Serialization;

namespace MVE;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$Event")]
[JsonDerivedType(typeof(AttachedEnemiesWaveEvent), "AttachedEnemies")]
[JsonDerivedType(typeof(ForwardWaveEvent), "Forward")]
[JsonDerivedType(typeof(PointsModifyWaveEvent), "PointsModify")]
[JsonDerivedType(typeof(WeightModifyWaveEvent), "WeightModify")]
public abstract class WaveEvent
{
    public abstract void Execute(Board board, ref int points);
}

public class AttachedEnemiesWaveEvent : WaveEvent
{
    public required int Row { get; set; }
    public required Sid Id { get; set; }

    public override void Execute(Board board, ref int points)
    {
        board.PlaceEnemy(Game.Instance.EnemyProperties[Id], Row);
    }
}

public class ForwardWaveEvent : WaveEvent
{
    public required string Id { get; set; }

    public override void Execute(Board board, ref int points)
    {
        if (board.LevelData.WaveEvent?.EventStores.TryGetValue(Id, out var e) is true)
        {
            if (e is not ForwardWaveEvent)
                e.Execute(board, ref points);
            else
                Game.Logger.LogWarn("ForwardWaveEvent", "Forward to a Forward event is not allowed.");
        }
    }
}

public class PointsModifyWaveEvent : WaveEvent
{
    public int? Set { get; set; }
    public int? Add { get; set; }

    public override void Execute(Board board, ref int points)
    {
        if (Add is not null)
            points += Add.Value;
        if (Set is not null)
            points = Set.Value;
    }
}

public class WeightModifyWaveEvent : WaveEvent
{
    public required Sid Id { get; set; }
    public int? Set { get; set; }
    public int? Add { get; set; }

    public override void Execute(Board board, ref int points)
    {
        var unit = board.LevelData.EnemiesSpawning.EnemyPool.First(u => u.Id == Id);
        if (Add is not null)
            unit.Weight += Add.Value;
        if (Set is not null)
            unit.Weight = Set.Value;
    }
}