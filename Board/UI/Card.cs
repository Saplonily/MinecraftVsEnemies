using Godot;

namespace MVE;

public partial class Card : BoardUI
{
    protected Sprite2D shadowMask = null!;
    protected Sprite2D contentSprite = null!;
    protected Area2D area = null!;

    protected bool mouseIn = false;

    [Export]
    public int WeaponPropertyId { get; set; } = -1;

    public CardState State { get; protected set; }

    public float Cooldown { get; protected set; } = 1.0f;

    public float CooldownStep { get; protected set; } = 1.0f / 100.0f;

    public WeaponProperty WeaponProperty { get; protected set; } = null!;

    public override void _Ready()
    {
        base._Ready();

        shadowMask = GetNode<Sprite2D>("ShadowMask");
        area = GetNode<Area2D>("Area2D");
        contentSprite = GetNode<Sprite2D>("Content");

        area.InputEvent += this.Area_InputEvent;
        area.MouseEntered += () => mouseIn = true;
        area.MouseExited += () => mouseIn = false;

        State = CardState.Cooldown;

        Cooldown = 0.0f;
        UpdateFromPropertyId(WeaponPropertyId);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        if (mouseIn)
        {
            Board.ExpectCursorShape = Game.Instance.Config.ReadyToPickCard;
        }
        switch (State)
        {
            case CardState.Idle:
            shadowMask.Visible = false;
            break;

            case CardState.Cooldown:
            shadowMask.Visible = true;
            Vector2 spriteSize = shadowMask.Texture.GetSize();
            shadowMask.RegionRect = new Rect2(Vector2.Zero, new Vector2(spriteSize.X, spriteSize.Y * Cooldown));
            Cooldown -= CooldownStep * (float)delta;
            if (Cooldown <= 0.0f)
                State = CardState.Idle;
            break;

            case CardState.Picked:
            shadowMask.Visible = true;
            Vector2 spriteSize2 = shadowMask.Texture.GetSize();
            shadowMask.RegionRect = new Rect2(Vector2.Zero, spriteSize2);
            break;
        }
    }

    private void Area_InputEvent(Node viewport, InputEvent ie, long shapeIdx)
    {
        if (State == CardState.Idle)
        {
            if (ie.IsActionPressed("pick_card"))
            {
                if (Board.Picking == PickingType.Idle)
                {
                    this.Pick();
                }
            }
        }
    }

    public void UpdateFromPropertyId(int wid)
    {
        wid = wid == -1 ? 0 : wid;
        var property = Game.Instance.WeaponProperties[wid];
        CooldownStep = 1.0f / property.Cooldown;
        contentSprite.Texture = property.ContentTexture;
        WeaponProperty = Game.Instance.WeaponProperties[WeaponPropertyId];
    }

    public void Pick()
    {
        Board.DoPick(PickingType.Card);
        Board.PickedCard = this;
        State = CardState.Picked;
    }

    public void UnPick()
    {
        Board.DoPick(PickingType.Idle);
        Board.PickedCard = null;
        State = CardState.Idle;
    }

    public void OnUsed()
    {
        this.UnPick();
        Cooldown = 1.0f;
        State = CardState.Cooldown;
    }

    public Texture2D GetShownTexture()
    {
        return contentSprite.Texture;
    }
}

public enum CardState
{
    Idle,
    Cooldown,
    Banned,
    Picked
}