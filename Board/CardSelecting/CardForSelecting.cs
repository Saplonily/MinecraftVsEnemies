using MVE.SalExt;

namespace MVE;

/// <summary>
/// 选卡界面的卡片
/// </summary>
public partial class CardForSelecting : Node2D
{
    public readonly PackedScene Scene = GD.Load<PackedScene>("res://Board/CardSelecting/CardForSelecting.tscn");

    [Export] protected Godot.Collections.Array<AudioStream> tapAudioStreams = default!;
    protected Chooser<AudioStreamPlayer> tapAudioPlayerChooser = default!;
    protected SelectingUI selectingUI = default!;
    protected TextureButton button = default!;
    protected Sprite2D contentSprite = default!;
    protected Label costLabel = default!;

    protected bool selected = false;
    public bool IsForSelectedDisplay { get; protected set; }
    public CardForSelecting? ForSelectedSource { get; protected set; }
    public WeaponProperty WeaponProperty { get; protected set; } = default!;
    public Sid WeaponPropertyId { get; protected set; }
    [Export]
    protected string WeaponPropertyIdEditor
    {
        get => WeaponPropertyId.ToString();
        set => WeaponPropertyId = value;
    }

    public override void _Ready()
    {
        button = GetNode<TextureButton>("TextureButton");
        contentSprite = GetNode<Sprite2D>("Content");
        costLabel = GetNode<Label>("Cost");
        selectingUI = this.FindParent<SelectingUI>() ?? throw new NodeNotFoundException(nameof(SelectingUI));
        tapAudioPlayerChooser = SalAudioPool.ChooserFromArray(tapAudioStreams, new(default!, Bus: "Board"));

        button.ButtonDown += this.Button_ButtonDown;
        UpdateFromPropertyId(WeaponPropertyId);
    }

    protected void Button_ButtonDown()
    {
        if (selected) return;
        tapAudioPlayerChooser.Choose().Play();
        if (!IsForSelectedDisplay)
        {
            ChangeSelectState(true);

            var newCard = Scene.Instantiate<CardForSelecting>();
            newCard.IsForSelectedDisplay = true;
            newCard.Position = Extensions.SwitchTransform(this, selectingUI);
            newCard.ForSelectedSource = this;
            newCard.WeaponPropertyId = WeaponPropertyId;
            selectingUI.AddChild(newCard);
            var targetPos = selectingUI.AddSelected(newCard);
            var tween = newCard.CreateTween()
                .SetEase(Tween.EaseType.Out)
                .SetTrans(Tween.TransitionType.Cubic);
            tween.TweenProperty(newCard, "position", targetPos, 0.3d);
            tween.TweenCallback(Callable.From(() =>
            {
                newCard.Switch2DParent(selectingUI.SelectedCardsNode2D);
            }));
        }
        else
        {
            this.Switch2DParent(selectingUI);
            var tween = CreateTween()
                .SetEase(Tween.EaseType.Out)
                .SetTrans(Tween.TransitionType.Cubic);
            tween.TweenProperty(this, "position", Extensions.SwitchTransform(ForSelectedSource!, selectingUI), 0.3d);
            tween.TweenCallback(Callable.From(() =>
            {
                QueueFree();
                ForSelectedSource!.ChangeSelectState(false);
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

    public void ChangeSelectState(bool value)
    {
        if (selected == value) return;
        button.MouseDefaultCursorShape =
            value ? Control.CursorShape.Arrow :
            Control.CursorShape.PointingHand;
        if (value)
            Modulate = (Modulate *= 0.6f) with { A = 1f };
        else
            Modulate = (Modulate /= 0.6f) with { A = 1f };
        selected = value;
    }
}
