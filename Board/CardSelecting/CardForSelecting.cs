namespace MVE;

/// <summary>
/// 选卡界面的卡片
/// </summary>
public partial class CardForSelecting : Node2D
{
    public readonly PackedScene Scene = GD.Load<PackedScene>("res://Board/CardSelecting/CardForSelecting.tscn");

    protected SelectingUI selectingUI = null!;
    protected TextureButton button = null!;

    protected bool selected = false;
    public bool IsForSelectedDisplay { get; protected set; }

    public override void _Ready()
    {
        button = GetNode<TextureButton>("TextureButton");
        selectingUI = this.FindParent<SelectingUI>() ?? throw new NodeNotFoundException(nameof(SelectingUI));
        button.ButtonDown += this.Button_ButtonDown;
    }

    public void Button_ButtonDown()
    {
        if (selected) return;
        if (!IsForSelectedDisplay)
        {
            ChangeSelectState(true);
            Vector2 nextPos = selectingUI.NextCardPosition;
            selectingUI.AddSelected(this);
            var newCard = Scene.Instantiate<CardForSelecting>();
            newCard.IsForSelectedDisplay = true;
            newCard.Position = Position;
            selectingUI.AddChild(newCard);
            newCard.CreateTween()
                .SetEase(Tween.EaseType.Out)
                .SetTrans(Tween.TransitionType.Cubic)
                .TweenProperty(newCard, "position", nextPos, 0.3d);
        }
        else
        {

        }


        void ChangeSelectState(bool value)
        {
            if (selected == value) return;
            button.MouseDefaultCursorShape =
                value ? Control.CursorShape.Arrow :
                Control.CursorShape.PointingHand;
            if (value)
                Modulate = (Modulate *= 0.8f) with { A = 1f };
            else
                Modulate = (Modulate /= 0.8f) with { A = 1f };
            selected = value;
        }
    }
}
