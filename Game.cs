using System.Text.Json;
using Saladim.SalLogger;

namespace MVE;

public partial class Game : Node
{
    protected Logger logger;

    public static Game Instance { get; protected set; } = default!;
    public static Logger Logger => Instance.logger;

    public SidLib<WeaponProperty> WeaponProperties { get; protected set; } = default!;
    public SidLib<EnemyProperty> EnemyProperties { get; protected set; } = default!;
    public SidLib<LevelSceneProperty> LevelSceneProperties { get; protected set; } = default!;
    public Version Version { get; protected set; }
    public UserConfig Config { get; protected set; }
    public Random Random => Random.Shared;

    public Game()
    {
        Instance = this;
        Version = new Version(0, 0, 1, 0);
        Config = new();
        logger = new LoggerBuilder()
            .WithAction(GD.Print)
            .WithLevelLimit(LogLevel.Trace)
            .Build();

        logger.LogInfo("Game", "Hello SalLogger in godot 4.0 .net7!");
        logger.LogInfo("Game", $"Current version: {Version}");

        logger.LogInfo("Game", "Loading WeaponProperties...");
        LoadWeaponProperties();

        logger.LogInfo("Game", "Loading EnemyProperties...");
        LoadEnemyProperties();

        logger.LogInfo("Game", "Loading LevelSceneProperties...");
        LoadLevelSceneProperties();
    }

    public static bool OnInterval(int frames)
        => Engine.GetProcessFrames() % (ulong)frames == 0;

    public static bool OnPhysicsInterval(int frames)
        => Engine.GetPhysicsFrames() % (ulong)frames == 0;

    public void ChangeSceneTo(Node sceneNode)
    {
        Callable.From<Node>(sceneNode =>
        {
            var tree = GetTree();
            tree.UnloadCurrentScene();
            tree.Root.AddChild(sceneNode);
            tree.CurrentScene = sceneNode;
        }).CallDeferred(sceneNode);
    }

    protected void LoadLevelSceneProperties()
    {
        LevelSceneProperties = new()
        {
            ["grasswalk"] = new(GD.Load<PackedScene>("res://TestLevel.tscn"))
        };
    }

    protected void LoadEnemyProperties()
    {
        EnemyProperties = new()
        {
            ["zombie"] = new(GD.Load<PackedScene>("res://Board/Enemies/Zombie/Zombie.tscn")),
            ["leat_zombie"] = new(GD.Load<PackedScene>("res://Board/Enemies/Zombie/Leat/LeatZombie.tscn")),
            ["iron_zombie"] = new(GD.Load<PackedScene>("res://Board/Enemies/Zombie/Iron/IronZombie.tscn"))
        };
    }

    protected void LoadWeaponProperties()
    {
        WeaponProperties = new()
        {
            ["dispenser"] = new(
                GD.Load<PackedScene>("res://Board/Weapons/Dispenser/Dispenser.tscn"),
                GD.Load<Texture2D>("res://Board/Weapons/Dispenser/Dispenser.png"),
                100, 5
            ),
            ["furnace"] = new(
                GD.Load<PackedScene>("res://Board/Weapons/Furnace/Furnace.tscn"),
                GD.Load<Texture2D>("res://Board/Weapons/Furnace/Furnace.png"),
                50, 5
            ),
            ["obsidian"] = new(
                GD.Load<PackedScene>("res://Board/Weapons/Obsidian/Obsidian.tscn"),
                GD.Load<Texture2D>("res://Board/Weapons/Obsidian/Obsidian.png"),
                50, 50
            )
        };
    }

    public void SwitchToLevelJson(string json)
    {
        try
        {
            var d = JsonSerializer.Deserialize<LevelData>(json)
                ?? throw new LevelLoadFailedException("LevelDataReading", "Failed to deserialize LevelData.");
            PackedScene scene = Instance.LevelSceneProperties[d.SceneId].Scene;
            var node = scene.Instantiate();
            if (node.GetNodeOrNull("Board") is not Board board)
                throw new LevelLoadFailedException("Node getting", "arg \"levelSceneRoot\" isn't a scene contains \"Board\" node directly.");
            board.LevelData = d;
            ChangeSceneTo(node);
        }
        catch (JsonException e)
        {
            throw new LevelLoadFailedException("LevelDataReading", "Failed to parse the json of LevelData.", e);
        }
    }

    public void SwitchToLevelResPath(string levelJsonResPath)
    {
        using GodotFileAccess gfa = GodotFileAccess.Open(levelJsonResPath, GodotFileAccess.ModeFlags.Read);
        string levelJson = gfa.GetAsText();
        gfa.Close();
        SwitchToLevelJson(levelJson);
    }

    public void SwitchToLevelNativePath(string levelJsonNativePath)
        => SwitchToLevelJson(System.IO.File.ReadAllText(levelJsonNativePath));
}
