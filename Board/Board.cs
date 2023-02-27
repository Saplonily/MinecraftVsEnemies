using NullLib.CommandLine;

namespace MVE;

public partial class Board
{
    protected PickingType picking;
    protected Sprite2D pickingSprite = null!;
    protected Control controlOverlay = null!;
    protected RedstoneDisplayer redstoneDisplayer = null!;

    [Export(PropertyHint.MultilineText)]
    public string StartUpCmds { get; set; } = null!;

    public CommandObject<MiscCmd> Command { get; set; } = null!;

    public Card? PickedCard { get; set; }

    public PickingType Picking { get => picking; }

    public Lawn Lawn { get; protected set; } = null!;

    public Control.CursorShape ExpectCursorShape { get; set; }

    public int ShadowZIndex => -1;

    public override void _Ready()
    {
        base._Ready();
        pickingSprite = GetNode<Sprite2D>("LayerPicking/Picking");
        controlOverlay = GetNode<Control>("LayerPicking/ControlOverlay");
        redstoneDisplayer = GetNode<RedstoneDisplayer>("LayerMain/RedstoneDisplayer");

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

        if (picking == PickingType.Card)
        {
            if (PickedCard != null)
            {
                pickingSprite.Texture = PickedCard.GetShownTexture();
                pickingSprite.Position = pickingSprite.GetGlobalMousePosition();
                Input.MouseMode = Input.MouseModeEnum.Hidden;
            }
        }

        controlOverlay.MouseDefaultCursorShape = ExpectCursorShape;
        ExpectCursorShape = Control.CursorShape.Arrow;
    }

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);
        if (@event is InputEventMouseButton mouseButton)
        {
            if (mouseButton.ButtonIndex == MouseButton.Right)
            {
                if (picking == PickingType.Card)
                {
                    DoPick(PickingType.Idle);
                    pickingSprite.Visible = false;
                    PickedCard!.UnPick();
                    PickedCard = null;
                }
            }
        }
    }

    public void DoPick(PickingType to)
    {
        var fromTo = (picking, to);
        picking = to;
        switch (fromTo)
        {
            case (PickingType.Idle, PickingType.Card):

            pickingSprite.Visible = true;

            break;

            case (PickingType.Card, PickingType.Idle):

            pickingSprite.Visible = false;
            pickingSprite.Position = Vector2.Zero;

            break;
        }
    }

    public DropPickResult RequestDropPicking(Drop drop)
    {
        if (drop is Redstone)
        {
            return DropPickResult.ToRedstoneDisplayer;
        }
        return DropPickResult.SelfDisappear;
    }
}

public enum PickingType
{
    Idle,
    Card,
    Pickaxe
}

public enum DropPickResult
{
    SelfDisappear,
    ToRedstoneDisplayer,
    ToMoneyBank
}