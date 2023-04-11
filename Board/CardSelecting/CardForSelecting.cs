using MVE.SalExt;

namespace MVE;

/// <summary>
/// 选卡界面的卡片
/// </summary>
public partial class CardForSelecting : Node2D
{
    public readonly PackedScene Scene = GD.Load<PackedScene>("res://Board/CardSelecting/CardForSelecting.tscn");

    [Export] protected Godot.Collections.Array<AudioStream> tapAudioStreams = default!;
    protected Label costLabel = default!;
    protected Sprite2D contentSprite = default!;
    protected SelectingUI selectingUI = default!;
    protected TextureButton button = default!;
    protected Chooser<AudioStreamPlayer> tapAudioPlayerChooser = default!;

    protected bool disabled = false;

    public bool IsForSelectedDisplay { get; protected set; }
    public CardForSelecting? ForSelectedSource { get; protected set; }
    public WeaponProperty WeaponProperty { get; protected set; } = default!;
    public Sid WeaponPropertyId { get; set; }

    public override void _Ready()
    {
        button = GetNode<TextureButton>("TextureButton");
        contentSprite = GetNode<Sprite2D>("Content");
        costLabel = GetNode<Label>("Cost");
        selectingUI = this.FindParent<SelectingUI>() ?? throw new NodeNotFoundException(nameof(SelectingUI));
        tapAudioPlayerChooser = SalAudioPool.GetChooserFromArray(tapAudioStreams, new(default!, Bus: "Board"));

        button.ButtonDown += this.Button_ButtonDown;
        UpdateFromPropertyId(WeaponPropertyId);
    }

    protected void Button_ButtonDown()
    {
        if (disabled) return;
        tapAudioPlayerChooser.Choose().Play();
        if (!IsForSelectedDisplay)
        {
            ChangeDisabledState(true);

            var newCard = Scene.Instantiate<CardForSelecting>();
            newCard.IsForSelectedDisplay = true;
            newCard.Position = Extensions.SwitchTransform(this, selectingUI);
            newCard.ForSelectedSource = this;
            newCard.WeaponPropertyId = WeaponPropertyId;
            selectingUI.AddChild(newCard);
            newCard.ChangeDisabledState(true, false);
            var targetPos = selectingUI.AddSelected(newCard);
            var tween = newCard.CreateTween()
                .SetEase(Tween.EaseType.Out)
                .SetTrans(Tween.TransitionType.Cubic);
            tween.TweenProperty(newCard, "position", targetPos, 0.3d);
            tween.TweenCallback(Callable.From(() =>
            {
                newCard.Switch2DParent(selectingUI.SelectedCardsNode2D);
                newCard.ChangeDisabledState(false, false);
            }));
        }
        else
        {
            ChangeDisabledState(true, false);
            this.Switch2DParent(selectingUI);
            var tween = CreateTween()
                .SetEase(Tween.EaseType.Out)
                .SetTrans(Tween.TransitionType.Cubic);
            tween.TweenProperty(this, "position", Extensions.SwitchTransform(ForSelectedSource!, selectingUI), 0.3d);
            tween.TweenCallback(Callable.From(() =>
            {
                ForSelectedSource!.ChangeDisabledState(false);
                QueueFree();
            }));
            selectingUI.RemoveSelected(this);
        }
    }

    public void UpdateFromPropertyId(Sid wid)
    {
        var property = Game.Instance.WeaponProperties[wid];
        contentSprite.Texture = property.ContentTexture;
        WeaponProperty = Game.Instance.WeaponProperties[WeaponPropertyId];
        costLabel.Text = property.Cost.ToString();
    }

    public void ChangeDisabledState(bool value, bool graify = true)
    {
        if (disabled == value) return;
        button.MouseDefaultCursorShape =
            value ? Control.CursorShape.Arrow :
            Control.CursorShape.PointingHand;
        if (graify)
        {
            if (value)
                CreateTween().TweenProperty(this, "modulate", (Modulate * 0.6f) with { A = 1f }, 0.1f);
            else
                Modulate = (Modulate / 0.6f) with { A = 1f };
        }
        disabled = value;
    }
}
