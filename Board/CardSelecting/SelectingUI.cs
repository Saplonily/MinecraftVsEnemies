namespace MVE;

/// <summary>
/// <para>选卡界面UI根</para>
/// <para></para>
/// </summary>
public partial class SelectingUI : Node2D
{
    public const float CardWidth = 64 * 0.9f;
    public const float CardForSelectingWidth = 60f;

    [Export] public PackedScene CardForSelectingScene = default!;
    public Node2D SelectedCardsNode2D = default!;
    public Node2D CardsForSelectingNode2D = default!;
    protected Button startButton = default!;
    protected Tween cardCancelMovingTween = default!;
    protected AnimationPlayer animationPlayer = default!;

    [Signal] public delegate void CardSelectingFinishedEventHandler(SelectingUI self);

    public List<(CardForSelecting card, Vector2 position)> SelectedCards { get; protected set; } = new();
    public List<Sid> CardsForSelecting { get; set; } = default!;


    public override void _Ready()
    {
        CardsForSelecting ??= new List<Sid>() { new("MVE", "dispenser") };
        SelectedCardsNode2D = GetNode<Node2D>("SelectedCardsUI/SelectedCards");
        CardsForSelectingNode2D = GetNode<Node2D>("ForSelectingUI/CardsForSelecting");
        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        startButton = GetNode<Button>("ForSelectingUI/StartButton");
        startButton.Pressed += this.StartButton_Pressed;

        Modulate = Modulate with { A = 0 };
        PlaceCardsForSelecting(CardsForSelecting);
    }

    protected void PlaceCardsForSelecting(List<Sid> cardSids)
    {
        for (int i = 0; i < cardSids.Count; i++)
        {
            var card = CardForSelectingScene.Instantiate<CardForSelecting>();
            card.WeaponPropertyId = cardSids[i];
            card.Position = Vector2.Right * i * CardForSelectingWidth;
            CardsForSelectingNode2D.AddChild(card);
        }
    }

    protected async void StartButton_Pressed()
    {
        EmitSignal(SignalName.CardSelectingFinished, this);
        animationPlayer.Play("Disappear");
        Variant[] args;
        do
        {
            args = await ToSignal(animationPlayer, AnimationPlayer.SignalName.AnimationFinished);
            if (args[0].AsStringName() == "Disappear")
                break;
        }
        while (true);
        QueueFree();
    }

    public void StartAppearAnimation()
    {
        animationPlayer.Play("Appear");
    }

    /// <summary>
    /// 添加一个选卡内容, 返回这个卡片应该缓动到的位置(相对ui)
    /// </summary>
    public Vector2 AddSelected(CardForSelecting card)
    {
        if (SelectedCards.Count == 0)
        {
            var pos = Extensions.SwitchTransform(SelectedCardsNode2D, this);
            SelectedCards.Add((card, Vector2.Zero));
            return pos;
        }
        else
        {
            Vector2 pos = SelectedCards[^1].position + Vector2.Right * CardWidth;
            Vector2 finalPos = Extensions.SwitchTransform(SelectedCardsNode2D, this, pos);
            SelectedCards.Add((card, pos));
            return finalPos;
        }
    }

    public void RemoveSelected(CardForSelecting card)
    {
        var index = SelectedCards.FindIndex(p => ReferenceEquals(p.card, card));
        if (index != -1)
        {
            if (SelectedCards.Count >= 2 && index != SelectedCards.Count - 1)
            {
                cardCancelMovingTween?.Kill();
                cardCancelMovingTween = CreateTween()
                    .SetParallel()
                    .SetEase(Tween.EaseType.Out)
                    .SetTrans(Tween.TransitionType.Quint);
                for (int i = SelectedCards.Count - 1; i >= index + 1; i--)
                {
                    cardCancelMovingTween.TweenProperty(SelectedCards[i].card, "position", SelectedCards[i - 1].position, 0.5d);
                    SelectedCards[i] = (SelectedCards[i].card, SelectedCards[i - 1].position);
                }
            }
            SelectedCards.RemoveAt(index);
        }
    }
}
