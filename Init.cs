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

        string levelJsonFile = "res://Levels/Level01.json";
        using GodotFileAccess gfa = GodotFileAccess.Open(levelJsonFile, GodotFileAccess.ModeFlags.Read);
        string levelJson = gfa.GetAsText();
        gfa.Close();
        try
        {
            LevelData? d = JsonSerializer.Deserialize<LevelData>(levelJson);
            if (d is null)
            {
                Game.Logger.LogError("LevelDataReading", "Failed to deserialize LevelData.");
                return;
            }
            s.GetNode<Board>("Board").LevelData = d;
        }
        catch (JsonException)
        {
            Game.Logger.LogError("LevelDataReading", "Failed to parse the LevelData json.");
        }
    }
}
