namespace MVE.SalExt;

public delegate void DrawingAction(RemoteDrawer drawer);

public partial class RemoteDrawer : Node2D
{
    protected DrawingAction? action;

    public void AssignAction(DrawingAction? action)
        => this.action = action;

    public override void _Process(double delta)
    {
        if (action is not null)
            QueueRedraw();
    }

    public override void _Draw()
    {
        action?.Invoke(this);
    }
}