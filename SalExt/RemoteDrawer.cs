namespace MVE.SalExt;

public delegate void DrawingAction(RemoteDrawer drawer);

public partial class RemoteDrawer : Node2D
{
    protected DrawingAction? action;
    protected bool drawOnce = false;
    protected bool drawn = false;

    public void AssignAction(DrawingAction? action, bool drawOnce = false)
        => (this.action, this.drawOnce) = (action, drawOnce);

    public override void _Process(double delta)
    {
        if (action is not null && (!drawOnce || (drawOnce && !drawn)))
        {
            QueueRedraw();
        }
    }

    public override void _Draw()
    {
        action?.Invoke(this);
    }
}