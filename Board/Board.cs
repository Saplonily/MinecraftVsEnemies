using System.Diagnostics;
using NullLib.CommandLine;

namespace MVE;

public partial class Board : Node
{
    protected Sprite2D pickingSprite = default!;
    protected RedstoneDisplayer redstoneDisplayer = default!;
    protected Label debugLabel = default!;
    protected BoardUIManager boardUIManager = default!;
    protected Camera2D camera = default!;
    protected CanvasLayer layerOverlay = default!;
    protected CanvasLayer layerMain = default!;
    protected DisplayServer.CursorShape preExpectedCursorShape;

    protected PickingType picking;

    public readonly Vector3 Gravity = new(0, 0, -1000);

    [Export(PropertyHint.MultilineText)]
    public string StartUpCmds { get; set; } = default!;
    public CommandObject<MiscCmd> Command { get; set; } = default!;
    public Node? PickedNode { get; set; }
    public PickingType Picking { get => picking; }
    public Lawn Lawn { get; protected set; } = default!;
    public DisplayServer.CursorShape ExpectCursorShape { get; set; }
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
        camera = GetNode<Camera2D>("LayerOverlay/Camera2D");
        layerMain = GetNode<CanvasLayer>("LayerMain");
        debugLabel = GetNode<Label>("LayerOverlay/Label");
        layerOverlay = GetNode<CanvasLayer>("LayerOverlay");
        pickingSprite = GetNode<Sprite2D>("LayerOverlay/Picking");
        boardUIManager = GetNode<BoardUIManager>("LayerOverlay/BoardUI");
        redstoneDisplayer = boardUIManager.RedstoneDisplayer;

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
        Input.MouseMode = Input.MouseModeEnum.Visible;

        if (picking is not PickingType.Idle && PickedNode is not null)
        {
            pickingSprite.Position = pickingSprite.GetGlobalMousePosition();
            Input.MouseMode = Input.MouseModeEnum.Hidden;
        }
        if (preExpectedCursorShape != ExpectCursorShape)
        {
            DisplayServer.CursorSetShape(ExpectCursorShape);
        }
        ExpectCursorShape = DisplayServer.CursorShape.Arrow;

        UpdateSpawner(delta);

        var tree = GetTree();
        debugLabel.Text = $"Enemy count: {tree.GetNodesInGroup("Enemy").Count}\n" +
            $"Weapon conut: {tree.GetNodesInGroup("Weapon").Count}\n" +
            $"Bullet count: {tree.GetNodesInGroup("Bullet").Count}\n" +
            $"Current wave: {CurrentWave}/{LevelData.TotalWaves}\n" +
            $"fps: {Engine.GetFramesPerSecond()}\n" +
            $"Wave timer: {sceneTreeTimer?.TimeLeft:F2}";
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
                if (sceneTreeTimer is not null)
                    sceneTreeTimer.TimeLeft = 0d;
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
        return Extensions.SwitchTransform(redstoneDisplayer, layerMain, redstoneDisplayer.RedstoneOffset);
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