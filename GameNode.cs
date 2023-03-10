using Saladim.SalLogger;

namespace MVE;

public partial class GameNode : Node
{
    public static GameNode Node { get; protected set; } = null!;

    public void ChangeSceneTo(Node sceneNode)
    {
        var tree = GetTree();
        tree.UnloadCurrentScene();
        tree.Root.AddChild(sceneNode);
        tree.CurrentScene = sceneNode;
    }

    public override void _Ready()
    {
        Node = this;
        _ = Game.Instance;
    }
}

public class Game
{
    protected Logger logger;

    public static Game Instance { get; protected set; }
    public static Logger Logger => Instance.logger;

    public List<WeaponProperty> WeaponProperties { get; protected set; } = null!;
    public List<EnemyProperty> EnemyProperties { get; protected set; } = null!;
    public Version Version { get; protected set; }
    public UserConfig Config { get; protected set; }
    public Random Random => Random.Shared;

    static Game()
    {
        Instance = new();
    }

    public Game()
    {
        logger = new LoggerBuilder()
            .WithAction(GD.Print)
            .WithLevelLimit(LogLevel.Trace)
            .Build();
        Version = new Version(0, 0, 1, 0);
        Config = new();

        logger.LogInfo("Game", "Hello SalLogger in godot 4.0 rc3 .net6!");
        logger.LogInfo("Game", $"Current version: {Version}");

        logger.LogInfo("Game", "Loading WeaponProperties...");
        LoadWeaponProperties();

        logger.LogInfo("Game", "Loading EnemyProperties...");
        LoadEnemyProperties();
    }

    public static bool OnInterval(int frames)
        => Engine.GetProcessFrames() % (ulong)frames == 0;

    public static bool OnPhysicsInterval(int frames)
        => Engine.GetPhysicsFrames() % (ulong)frames == 0;

    public void LoadEnemyProperties()
    {
        EnemyProperties = new();
        var p = EnemyProperties;
        p.Add(new EnemyProperty(GD.Load<PackedScene>("res://Board/Enemies/Zombie/Zombie.tscn")));
        p.Add(new EnemyProperty(GD.Load<PackedScene>("res://Board/Enemies/Zombie/Leat/LeatZombie.tscn")));
        p.Add(new EnemyProperty(GD.Load<PackedScene>("res://Board/Enemies/Zombie/Iron/IronZombie.tscn")));
    }

    public void LoadWeaponProperties()
    {
        WeaponProperties = new();
        var p = WeaponProperties;
        p.Add(new WeaponProperty(
            GD.Load<PackedScene>("res://Board/Weapons/Dispenser/Dispenser.tscn"),
            GD.Load<Texture2D>("res://Board/Weapons/Dispenser/Dispenser.png"),
            100, 5
            ));
        p.Add(new WeaponProperty(
            GD.Load<PackedScene>("res://Board/Weapons/Furnace/Furnace.tscn"),
            GD.Load<Texture2D>("res://Board/Weapons/Furnace/Furnace.png"),
            50, 5)
            );
        p.Add(new WeaponProperty(
            GD.Load<PackedScene>("res://Board/Weapons/Obsidian/Obsidian.tscn"),
            GD.Load<Texture2D>("res://Board/Weapons/Obsidian/Obsidian.png"),
            50, 50
            ));
    }

    public static class Weapons
    {
        public const int Dispenser = 0;
        public const int Furnace = 1;
    }

    public static class Enemies
    {
        public const int Zombie = 0;
    }
}