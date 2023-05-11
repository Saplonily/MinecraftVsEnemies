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

    protected const byte ColorByte = (byte)(255 * 0.6f);

    public GodotColor SelfMaskColor { get; set; } = GodotColor.Color8(ColorByte, ColorByte, ColorByte, 0xff);
    public Sid WeaponPropertyId { get; set; } = "dispenser";
    public double Cooldown { get; set; } = 1.0;
    public double CooldownStep { get; protected set; } = 1.0 / 100.0;

    [Export] public string? InitWeaponSid { get => WeaponPropertyId.ToString(); set => WeaponPropertyId = Sid.Parse(value!); }
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

        Cooldown = 0.0f;
        UpdateFromPropertyId(WeaponPropertyId);
        pickShownConfig = new()
        {
            Texture = contentSprite.Texture,
            Transform = Transform2D.Identity
        };
        tapAudiosPlayerChooser = SalAudioPool.GetChooserFromArray(tapAudios, (new(default!, Bus: "Board")));

        stateMachine = new(CardState.Idle);
        stateMachine.RegisterState(CardState.Idle, IdleEnter, update: IdleUpdate);
        stateMachine.RegisterState(CardState.Cooldown, update: CooldownUpdate);
        stateMachine.RegisterState(CardState.Picked, PickedEnter, PickedExit);
        stateMachine.EnterCurrent();
    }

    protected void PickedEnter(CardState s)
    {
        SetMaskState(true);
        pickAudioPlayer.Play();
    }

    protected void PickedExit(CardState s)
    {
        SetMaskState(false);
    }

    protected void SetMaskState(bool basicMask, float cooldownMaskPercent = 0f)
    {
        shadowMask.Visible = basicMask;
        Modulate = basicMask ? SelfMaskColor : Colors.White;
        var rect = cardSprite.GetRect();
        shadowMask.RegionRect = rect with { Size = new(rect.Size.X, rect.Size.Y * cooldownMaskPercent) };
    }

    protected bool IsAvailable()
    {
        if (Board.Bank.Redstone < WeaponProperty.Cost || Disabled)
        {
            return false;
        }
        return true;
    }

    protected void IdleEnter(CardState s)
        => stateMachine.Update(GetProcessDeltaTime());

    protected void IdleUpdate(double delta)
        => SetMaskState(!IsAvailable());

    public override void OnDisabledChanged(bool isDisabled)
        => SetMaskState(!IsAvailable());

    protected void CooldownUpdate(double delta)
    {
        Cooldown -= CooldownStep * delta;
        if (Cooldown <= 0.0f)
            stateMachine.State = CardState.Idle;
        SetMaskState(true, (float)Cooldown);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        stateMachine.Update(delta);
        if (!Disabled && mouseIn && stateMachine.State is CardState.Idle or CardState.Cooldown)
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
            tapAudiosPlayerChooser.Choose().Play();
            stateMachine.State = CardState.Idle;
        }
        if (travelType is PickingTravelType.Used)
        {
            Cooldown = 1.0f;
            stateMachine.State = CardState.Cooldown;
            Board.Bank.ReduceRedstone(WeaponProperty.Cost);
        }
    }

    public void OnPicked(PickingType source, PickingTravelType travelType)
        => stateMachine.State = CardState.Picked;

    public void OnUsed()
    {
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
