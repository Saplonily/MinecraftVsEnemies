namespace MVE;

public partial class BoardUI : Node2D
{
    protected bool disabled = false;

    public bool Disabled
    {
        get => disabled;
        set
        {
            if (disabled != value)
            {
                disabled = !disabled;
                OnDisabledChanged(disabled);
            }
        }
    }

    public Board Board { get; protected set; } = default!;

    public override void _Ready()
    {
        Board = this.FindParent<Board>() ?? throw new NodeNotFoundException(nameof(MVE.Board));
    }

    public virtual void OnDisabledChanged(bool isDisabled) { }
}
