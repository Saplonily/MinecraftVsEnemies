namespace MVE;

/// <summary>
/// <para>选卡界面UI根</para>
/// <para></para>
/// </summary>
public partial class SelectingUI : Node2D
{
    public const float CardWidth = 64 * 0.9f;

    [Export] public Vector2 SelectedCardsOrigin { get; set; }
    [Export] public Vector2 ForSelectingCardsOrigin { get; set; }
    public List<CardForSelecting> SelectedCards { get; protected set; } = new();
    public int SelectedCount => SelectedCards.Count;

    public Vector2 NextCardPosition { get; protected set; }

    public override void _Ready()
    {
        NextCardPosition = SelectedCardsOrigin;
    }

    public void AddSelected(CardForSelecting card)
    {
        NextCardPosition += Vector2.Right * CardWidth;
        SelectedCards.Add(card);
    }
}