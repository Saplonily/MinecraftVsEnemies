namespace MVE;

public partial class Pickaxe : BoardUI, IBoardUIPickable
{
    protected PickShownConfig pickShownConfig = null!;
    protected Sprite2D contentSprite = null!;
    protected Area2D area2D = null!;

    protected bool mouseIn = false;
    protected bool picked = false;

    public override void _Ready()
    {
        base._Ready();

        area2D = GetNode<Area2D>("Area2D");
        contentSprite = GetNode<Sprite2D>("DiamondPickaxe");

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
            Board.DoPick(PickingType.Pickaxe, this);
        }
    }

    PickShownConfig IBoardUIPickable.GetShownConfig()
        => pickShownConfig;

    public void OnUsed()
    {
        Board.DoPick(PickingType.Idle, null);
        picked = false;
    }

    void IBoardUIPickable.OnPicked()
    {
        picked = true;
    }

    void IBoardUIPickable.OnUnpicked()
    {
        picked = false;
    }
}
