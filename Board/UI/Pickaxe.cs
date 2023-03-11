using MVE.SalExt;

namespace MVE;

public partial class Pickaxe : BoardUI, IBoardUIPickable
{
    [Export] protected AudioStream audioPick = null!;
    [Export] protected AudioStream audioUnpick = null!;
    protected AudioStreamPlayer audioPickPlayer = null!;
    protected AudioStreamPlayer audioUnpickPlayer = null!;

    protected Sprite2D contentSprite = null!;
    protected Area2D area2D = null!;

    protected PickShownConfig pickShownConfig = null!;
    protected bool mouseIn = false;
    protected bool picked = false;

    public override void _Ready()
    {
        base._Ready();

        area2D = GetNode<Area2D>("Area2D");
        contentSprite = GetNode<Sprite2D>("DiamondPickaxe");
        audioPickPlayer = SalAudioPool.GetPlayer(new(audioPick, Bus: "Board"));
        audioUnpickPlayer = SalAudioPool.GetPlayer(new(audioUnpick, Bus: "Board"));

        pickShownConfig = new(contentSprite);

        area2D.MouseEntered += () => mouseIn = true;
        area2D.MouseExited += () => mouseIn = false;
        area2D.InputEvent += this.Area2D_InputEvent;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        if (mouseIn)
        {
            Board.ExpectCursorShape = Game.Instance.Config.CursorShapeReadyToPickPickaxe;
        }
        contentSprite.Visible = !picked;
    }

    protected void Area2D_InputEvent(Node viewport, InputEvent ie, long shapeIdx)
    {
        if (!picked && ie.IsActionPressed(InputNames.PickPickaxe) && Board.Picking is PickingType.Idle)
        {
            picked = true;
            Board.DoPick(PickingType.Pickaxe, PickingTravelType.PlayerSelect, this);
        }
    }

    PickShownConfig IBoardUIPickable.GetShownConfig()
        => pickShownConfig;

    public void OnUsed()
    {
        picked = false;
        Board.DoPick(PickingType.Idle, PickingTravelType.Used, null);
    }

    void IBoardUIPickable.OnPicked(PickingType source, PickingTravelType travelType)
    {
        picked = true;
        audioPickPlayer.Play();
    }

    void IBoardUIPickable.OnUnpicked(PickingType source, PickingTravelType travelType)
    {
        picked = false;
        if (travelType is PickingTravelType.PlayerCancel)
        {
            audioUnpickPlayer.Play();
        }
    }
}
