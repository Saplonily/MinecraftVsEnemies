using Godot;
using SalState;

namespace MVE;

public partial class Card : BoardUI
{
    protected Sprite2D cardSprite = null!;
    protected Sprite2D contentSprite = null!;
    protected Sprite2D shadowMask = null!;
    protected Area2D area = null!;
    protected Label costLabel = null!;

    protected StateMachine<CardState> stateMachine = null!;
    protected bool mouseIn = false;

    [Export]
    public Color SelfMaskColor { get; set; } = Color.Color8(143, 143, 143, 255);

    [Export]
    public int WeaponPropertyId { get; set; } = -1;

    public double Cooldown { get; protected set; } = 1.0;

    public double CooldownStep { get; protected set; } = 1.0 / 100.0;

    public WeaponProperty WeaponProperty { get; protected set; }

    public override void _Ready()
    {
        base._Ready();
        area = GetNode<Area2D>("Area2D");
        contentSprite = GetNode<Sprite2D>("Content");
        cardSprite = GetNode<Sprite2D>("Card");
        shadowMask = GetNode<Sprite2D>("ShadowMask");
        costLabel = GetNode<Label>("CostLabel");

        area.InputEvent += this.Area_InputEvent;
        area.MouseEntered += () => mouseIn = true;
        area.MouseExited += () => mouseIn = false;

        stateMachine = new(CardState.Cooldown);
        stateMachine.RegisterState(CardState.Idle,
            enter: IdleEnter,
            update: IdleUpdate
            );
        stateMachine.RegisterState(CardState.Cooldown,
            enter: _ => MakeMasked(),
            exit: _ => RestoreMask(),
            update: CooldownUpdate
            );
        stateMachine.RegisterState(CardState.Picked,
            enter: _ => Modulate = SelfMaskColor,
            exit: _ => Modulate = Color.Color8(255, 255, 255, 255)
            );

        Cooldown = 0.0f;
        UpdateFromPropertyId(WeaponPropertyId);
    }

    protected void MakeMasked()
    {
        (shadowMask.Visible, Modulate) = (true, SelfMaskColor);
        UpdateMaskRegion(0f);
    }

    protected void RestoreMask()
    {
        (shadowMask.Visible, Modulate) = (false, Color.Color8(255, 255, 255, 255));
        UpdateMaskRegion(0f);
    }

    protected bool IsAvailable()
    {
        if (Board.Bank.Redstone < WeaponProperty.Cost)
        {
            return false;
        }
        return true;
    }

    protected void IdleEnter(CardState s)
    {
        if (IsAvailable())
        {
            RestoreMask();
        }
        else
        {
            MakeMasked();
        }
    }

    protected void IdleUpdate(double delta)
    {
        if (IsAvailable())
        {
            RestoreMask();
        }
        else
        {
            MakeMasked();
        }
    }

    protected void UpdateMaskRegion(float percent)
    {
        var rect = cardSprite.GetRect();
        shadowMask.RegionRect = rect with { Size = new(rect.Size.X, rect.Size.Y * percent) };
    }

    protected void CooldownUpdate(double delta)
    {
        Cooldown -= CooldownStep * delta;
        if (Cooldown <= 0.0f)
            stateMachine.State = CardState.Idle;
        UpdateMaskRegion((float)Cooldown);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        stateMachine.Update(delta);
        if (mouseIn && stateMachine.State is CardState.Idle or CardState.Cooldown)
        {
            Board.ExpectCursorShape = Game.Instance.Config.CursorShapeReadyToPickCard;
        }
    }

    protected void Area_InputEvent(Node viewport, InputEvent ie, long shapeIdx)
    {
        if (stateMachine.State is CardState.Idle && ie.IsActionPressed(InputNames.PickCard))
        {
            if (IsAvailable() && Board.Picking == PickingType.Idle)
            {
                this.Pick();
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
        costLabel.Text = property.Cost.ToString();
    }

    public void Pick()
    {
        Board.DoPick(PickingType.Card);
        Board.PickedCard = this;
        stateMachine.State = CardState.Picked;
    }

    public void UnPick()
    {
        Board.DoPick(PickingType.Idle);
        Board.PickedCard = null;
        stateMachine.State = CardState.Idle;
    }

    public void OnUsed()
    {
        this.UnPick();
        Cooldown = 1.0f;
        stateMachine.State = CardState.Cooldown;
        Board.Bank.ReduceRedstone(WeaponProperty.Cost);
    }

    public Texture2D GetShownTexture()
    {
        return contentSprite.Texture;
    }
}

public enum CardState
{
    Idle,
    Disabled,
    Cooldown,
    Picked
}