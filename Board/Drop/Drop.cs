namespace MVE;

public partial class Drop : LawnEntity
{
    protected bool mouseIn = false;
    protected bool picked = false;
    protected Area2D hitBox = null!;

    public override void _Ready()
    {
        base._Ready();
        hitBox = GetNode<Area2D>("HitBox");

        hitBox.MouseEntered += () => mouseIn = true;
        hitBox.MouseExited += () => mouseIn = false;
        hitBox.InputEvent += this.HitBox_InputEvent;
    }

    private void HitBox_InputEvent(Node viewport, InputEvent e, long shapeIdx)
    {
        if (e.IsActionPressed(InputNames.PickDrop))
        {
            OnPicking();
        }
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        if (mouseIn && OnHover())
        {
            Board.ExpectCursorShape = Game.Instance.Config.CursorShapeReadyToPick;
        }
    }

    /// <summary>
    /// 尝试Pick
    /// </summary>
    public virtual void OnPicking()
    {
        picked = true;
    }

    /// <summary>
    /// 当鼠标悬浮时应该显示手指图标
    /// </summary>
    /// <returns></returns>
    public virtual bool OnHover()
    {
        return true;
    }
}
