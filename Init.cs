using System.Text.Json;

namespace MVE;

public partial class Init : Node
{
    [Export] protected PackedScene initGotoScene = default!;

    public override void _Ready()
    {
        CallDeferred(MethodName.Unload);
    }

    public void Unload()
    {
        var s = initGotoScene.Instantiate();
        GameNode.Node.ChangeSceneTo(s);

        if (s.GetNodeOrNull("Board") is not Board board)
            return;

        string levelJsonFile = "res://Levels/Level01.json";
        using GodotFileAccess gfa = GodotFileAccess.Open(levelJsonFile, GodotFileAccess.ModeFlags.Read);
        string levelJson = gfa.GetAsText();
        gfa.Close();
        try
        {
            var d = JsonSerializer.Deserialize<LevelData>(levelJson);
            if (d is null)
            {
                Game.Logger.LogError("LevelDataReading", "Failed to deserialize LevelData.");
                return;
            }
            board.LevelData = d;
        }
        catch (JsonException)
        {
            Game.Logger.LogError("LevelDataReading", "Failed to parse the LevelData json.");
        }
    }
}
