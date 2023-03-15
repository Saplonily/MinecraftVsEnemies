using System.Diagnostics;
using System.Text;
using NullLib.CommandLine;

namespace MVE;

public partial class Board : Node
{
    protected Sprite2D pickingSprite = null!;
    protected Control controlOverlay = null!;
    protected RedstoneDisplayer redstoneDisplayer = null!;
    protected Label debugLabel = null!;

    protected PickingType picking;

    public readonly Vector3 Gravity = new(0, 0, -1000);

    [Export(PropertyHint.MultilineText)]
    public string StartUpCmds { get; set; } = null!;
    public CommandObject<MiscCmd> Command { get; set; } = null!;
    public Node? PickedNode { get; set; }
    public PickingType Picking { get => picking; }
    public Lawn Lawn { get; protected set; } = null!;
    public Control.CursorShape ExpectCursorShape { get; set; }
    public BoardBank Bank { get; protected set; }
    public Random Random { get; set; }
    public int ShadowZIndex => -1;

    public delegate void PickingChangedEventHandler(PickingType from, PickingType to);
    public event PickingChangedEventHandler? PickingChanged;

    public Board()
    {
        Bank = new();
        Random = Random.Shared;
    }

    public override void _Ready()
    {
        base._Ready();
        pickingSprite = GetNode<Sprite2D>("LayerPicking/Picking");
        controlOverlay = GetNode<Control>("LayerPicking/ControlOverlay");
        redstoneDisplayer = GetNode<RedstoneDisplayer>("LayerMain/BoardUI/RedstoneDisplayer");
        debugLabel = GetNode<Label>("LayerPicking/Label");
        InitAudios();
        InitSpawner();

        Lawn = GetNode<Lawn>("LayerMain/Lawn");

        picking = PickingType.Idle;
        pickingSprite.Visible = false;
        Command = new(new MiscCmd(this, Lawn));
        if (StartUpCmds is not null)
        {
            foreach (var cmd in StartUpCmds.Split('\n'))
            {
                Command.TryExecuteCommand(cmd, out _);
            }
        }
    }


    public override void _Process(double delta)
    {
        base._Process(delta);
        Input.MouseMode = Input.MouseModeEnum.Visible;

        if (picking is not PickingType.Idle && PickedNode is not null)
        {
            pickingSprite.Position = pickingSprite.GetGlobalMousePosition();
            Input.MouseMode = Input.MouseModeEnum.Hidden;
        }

        controlOverlay.MouseDefaultCursorShape = ExpectCursorShape;
        ExpectCursorShape = Control.CursorShape.Arrow;

        UpdateSpawner(delta);

        var tree = GetTree();
        debugLabel.Text = $"Enemy count: {tree.GetNodesInGroup("Enemy").Count}\n" +
            $"Weapon conut: {tree.GetNodesInGroup("Weapon").Count}\n" +
            $"Bullet count: {tree.GetNodesInGroup("Bullet").Count}\n" +
            $"Current wave: {CurrentWave}/{LevelData.TotalWaves}\n" +
            $"fps: {Engine.GetFramesPerSecond()}\n" +
            $"Wave timer: {spawnerTimer.TimeLeft:F2}";

    }

    [Conditional("DEBUG")]
    public void HandleDebugInputs(InputEvent ie)
    {
        if (ie is InputEventKey key && !key.IsEcho() && key.Pressed)
        {
            if (key.Keycode == Key.P)
            {
                Command.TargetInstance.Produce();
            }

            if (key.Keycode == Key.K)
            {
                Command.TargetInstance.KillAll();
            }

            if (key.Keycode == Key.N)
            {
                SpawnerTimerTimeout();
            }

            if (key.Keycode == Key.H)
            {
                foreach (var card in GetTree().GetNodesInGroup("Card").Cast<Card>())
                {
                    card.Cooldown = 0d;
                }
            }

            if (key.Keycode == Key.M)
            {
                Bank.SetRedstone(2500);
            }
        }
    }

    public override void _Input(InputEvent ie)
    {
        base._Input(ie);
        HandleDebugInputs(ie);
        if (ie is InputEventMouseButton mouseButton)
        {
            if (mouseButton.ButtonIndex is MouseButton.Right)
            {
                if (picking is not PickingType.Idle && PickedNode is IBoardUIPickable)
                {
                    DoPick(PickingType.Idle, PickingTravelType.PlayerCancel, null);
                }
            }
        }
    }

    public void DoPick(PickingType to, PickingTravelType travelType, Node? pickedNode)
    {
        var from = picking;
        picking = to;
        var fromNode = PickedNode;
        PickedNode = pickedNode;
        if (from is PickingType.Idle && to is not PickingType.Idle)
        {
            if (pickedNode is IBoardUIPickable bup)
            {
                pickingSprite.Visible = true;
                bup.GetShownConfig().ApplyToSprite(pickingSprite);
                bup.OnPicked(from, travelType);
            }
        }
        if (from is not PickingType.Idle && to is PickingType.Idle)
        {
            pickingSprite.Visible = false;
            if (fromNode is IBoardUIPickable bup)
            {
                bup.OnUnpicked(from, travelType);
            }
        }
        PickingChanged?.Invoke(from, to);
    }

    public DropPickResult RequestDropPicking(Drop drop)
    {
        if (drop is Redstone)
        {
            return DropPickResult.ToRedstoneDisplayer;
        }
        return DropPickResult.SelfDisappear;
    }

    public Vector2 GetRedstoneDisplayerSlotPos()
    {
        return redstoneDisplayer.Position + redstoneDisplayer.RedstoneOffset;
    }
}

public enum PickingType
{
    Idle,
    Card,
    Pickaxe
}

public enum PickingTravelType
{
    None,
    PlayerCancel,
    PlayerSelect,
    Used
}

public enum DropPickResult
{
    SelfDisappear,
    ToRedstoneDisplayer,
    ToMoneyBank
}