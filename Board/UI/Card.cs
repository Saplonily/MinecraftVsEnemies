using MVE.SalExt;

namespace MVE;

public partial class Card : BoardUI, IBoardUIPickable
{
    [Export] protected AudioStream pickAudio = default!;
    protected AudioStreamPlayer pickAudioPlayer = default!;
    [Export] protected Godot.Collections.Array<AudioStream> tapAudios = default!;
    protected Chooser<AudioStreamPlayer> tapAudiosPlayerChooser = default!;

    protected Sprite2D cardSprite = default!;
    protected Sprite2D contentSprite = default!;
    protected Sprite2D shadowMask = default!;
    protected Area2D area = default!;
    protected Label costLabel = default!;


    protected PickShownConfig pickShownConfig = default!;
    protected StateMachine<CardState> stateMachine = default!;
    protected bool mouseIn = false;

    [Export]
    public Color SelfMaskColor { get; set; } = Color.Color8(143, 143, 143, 255);

    public Sid WeaponPropertyId { get; set; } = "dispenser";

    public double Cooldown { get; set; } = 1.0;

    public double CooldownStep { get; protected set; } = 1.0 / 100.0;

    public WeaponProperty WeaponProperty { get; protected set; } = default!;

    public override void _Ready()
    {
        base._Ready();
        area = GetNode<Area2D>("Area2D");
        contentSprite = GetNode<Sprite2D>("Content");
        cardSprite = GetNode<Sprite2D>("Card");
        shadowMask = GetNode<Sprite2D>("ShadowMask");
        costLabel = GetNode<Label>("CostLabel");
        pickAudioPlayer = SalAudioPool.GetPlayer(new(pickAudio, Bus: "UI"));

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
            enter: PickedEnter,
            exit: _ => Modulate = Color.Color8(255, 255, 255, 255)
            );

        Cooldown = 0.0f;
        UpdateFromPropertyId(WeaponPropertyId);
        pickShownConfig = new()
        {
            Texture = contentSprite.Texture,
            Transform = Transform2D.Identity
        };
        tapAudiosPlayerChooser = SalAudioPool.ChooserFromArray(tapAudios, (new(default!, Bus: "Board")));
    }

    protected void PickedEnter(CardState s)
    {
        Modulate = SelfMaskColor;
        pickAudioPlayer.Play();
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
            RestoreMask();
        else
            MakeMasked();
    }

    protected void IdleUpdate(double delta)
    {
        if (IsAvailable())
            RestoreMask();
        else
            MakeMasked();
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
        if (stateMachine.State is CardState.Idle && ie.IsActionPressed(InputNames.Using))
        {
            if (IsAvailable() && Board.Picking is PickingType.Idle)
            {
                Board.DoPick(PickingType.Card, PickingTravelType.PlayerSelect, this);
                stateMachine.State = CardState.Picked;
            }
        }
    }

    public void UpdateFromPropertyId(Sid wid)
    {
        var property = Game.Instance.WeaponProperties[wid];
        CooldownStep = 1.0f / property.Cooldown;
        contentSprite.Texture = property.ContentTexture;
        WeaponProperty = Game.Instance.WeaponProperties[WeaponPropertyId];
        costLabel.Text = property.Cost.ToString();
    }

    public void OnUnpicked(PickingType source, PickingTravelType travelType)
    {
        if (travelType is PickingTravelType.PlayerCancel)
        {
            stateMachine.State = CardState.Idle;
            tapAudiosPlayerChooser.Choose().Play();
        }
    }

    public void OnPicked(PickingType source, PickingTravelType travelType)
        => stateMachine.State = CardState.Picked;

    public void OnUsed()
    {
        Cooldown = 1.0f;
        stateMachine.State = CardState.Cooldown;
        Board.Bank.ReduceRedstone(WeaponProperty.Cost);
        Board.DoPick(PickingType.Idle, PickingTravelType.Used, null);
    }

    PickShownConfig IBoardUIPickable.GetShownConfig()
        => pickShownConfig;
}

public enum CardState
{
    Idle,
    Disabled,
    Cooldown,
    Picked
}
