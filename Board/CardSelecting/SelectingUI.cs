namespace MVE;

/// <summary>
/// <para>选卡界面UI根</para>
/// <para></para>
/// </summary>
public partial class SelectingUI : Node2D
{
    public const float CardWidth = 64 * 0.9f;
    protected Tween cardCancelMovingTween = null!;

    [Export] public Vector2 SelectedCardsOrigin { get; set; }
    [Export] public Vector2 ForSelectingCardsOrigin { get; set; }
    public List<(CardForSelecting card, Vector2 position)> SelectedCards { get; protected set; } = new();
    public int SelectedCount => SelectedCards.Count;


    public override void _Ready()
    {
    }

    public Vector2 AddSelected(CardForSelecting card)
    {
        if (SelectedCards.Count == 0)
        {
            SelectedCards.Add((card, SelectedCardsOrigin));
            return SelectedCardsOrigin;
        }
        else
        {
            Vector2 pos = SelectedCards[^1].position + Vector2.Right * CardWidth;
            SelectedCards.Add((card, pos));
            return pos;
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