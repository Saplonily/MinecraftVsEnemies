namespace MVE;

/// <summary>
/// 选卡界面的卡片
/// </summary>
public partial class CardForSelecting : Node2D
{
    public readonly PackedScene Scene = GD.Load<PackedScene>("res://Board/CardSelecting/CardForSelecting.tscn");

    protected SelectingUI selectingUI = null!;
    protected TextureButton button = null!;
    protected Sprite2D contentSprite = null!;
    protected Label costLabel = null!;

    protected bool selected = false;
    public bool IsForSelectedDisplay { get; protected set; }
    public CardForSelecting? ForSelectedSource { get; protected set; }
    public WeaponProperty WeaponProperty { get; protected set; } = null!;
    [Export] public int WeaponPropertyInitId { get; protected set; }

    public override void _Ready()
    {
        button = GetNode<TextureButton>("TextureButton");
        contentSprite = GetNode<Sprite2D>("Content");
        costLabel = GetNode<Label>("Cost");
        selectingUI = this.FindParent<SelectingUI>() ?? throw new NodeNotFoundException(nameof(SelectingUI));
        button.ButtonDown += this.Button_ButtonDown;
        UpdateFromPropertyId(WeaponPropertyInitId);
    }

    protected void Button_ButtonDown()
    {
        if (selected) return;
        if (!IsForSelectedDisplay)
        {
            ChangeSelectState(true);

            var newCard = Scene.Instantiate<CardForSelecting>();
            newCard.IsForSelectedDisplay = true;
            newCard.Position = Position;
            newCard.ForSelectedSource = this;
            newCard.WeaponPropertyInitId = WeaponPropertyInitId;
            selectingUI.AddChild(newCard);
            var targetPos = selectingUI.AddSelected(newCard);
            newCard.CreateTween()
                .SetEase(Tween.EaseType.Out)
                .SetTrans(Tween.TransitionType.Cubic)
                .TweenProperty(newCard, "position", targetPos, 0.3d);
        }
        else
        {
            var tween = CreateTween()
                .SetEase(Tween.EaseType.Out)
                .SetTrans(Tween.TransitionType.Cubic);
            tween.TweenProperty(this, "position", ForSelectedSource!.Position, 0.3d);
            tween.TweenCallback(Callable.From(() =>
            {
                QueueFree();
                ForSelectedSource!.ChangeSelectState(false);
            }));
            selectingUI.RemoveSelected(this);
        }
    }

    public void UpdateFromPropertyId(int wid)
    {
        wid = wid == -1 ? 0 : wid;
        var property = Game.Instance.WeaponProperties[wid];
        contentSprite.Texture = property.ContentTexture;
        WeaponProperty = Game.Instance.WeaponProperties[WeaponPropertyInitId];
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
