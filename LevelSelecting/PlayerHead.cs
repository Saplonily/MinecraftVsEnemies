namespace MVE.LevelSelecting;

public partial class PlayerHead : Node2D
{
    protected StopStone currentStopStone = null!;
    protected Tween? movingTween;
    protected RayCast2D rayCast = null!;

    public override void _Ready()
    {
        rayCast = this.GetNode<RayCast2D>("RayCast2D");
        rayCast.Enabled = false;
        CallDeferred(MethodName.ReadyDeferred);
    }

    public void ReadyDeferred()
    {
        rayCast.HitFromInside = true;
        rayCast.TargetPosition = default;
        rayCast.ForceRaycastUpdate();
        StopStone? stopStone = rayCast.GetCollider().AsCast<Node>()?.Owner.AsCast<StopStone>();
        rayCast.HitFromInside = false;
        currentStopStone = stopStone ?? throw new NodeNotFoundException(nameof(StopStone));
    }
    public override void _PhysicsProcess(double delta)
    {
        Vector2I dirVec = default;
        if (Input.IsActionPressed(InputNames.MoveLeft))
            dirVec = Vector2I.Left;
        if (Input.IsActionPressed(InputNames.MoveRight))
            dirVec = Vector2I.Right;
        if (Input.IsActionPressed(InputNames.MoveUp))
            dirVec = Vector2I.Up;
        if (Input.IsActionPressed(InputNames.MoveDown))
            dirVec = Vector2I.Down;

        if (dirVec == default) return;

        rayCast.TargetPosition = dirVec * 1024;
        rayCast.ForceRaycastUpdate();

        if (movingTween?.IsValid() is not true && rayCast.GetCollider().AsCast<Node>()?.Owner is StopStone stopStone)
        {
            if (StopStone.TryGetDirection(dirVec, out StopStone.Direction dir))
            {
                if ((dir & currentStopStone.DirectionAllowed) != StopStone.Direction.None)
                {
                    float dis = stopStone.Position.DistanceTo(Position);
                    movingTween = this.CreateTween().SetEase(Tween.EaseType.In).SetTrans(Tween.TransitionType.Linear);
                    movingTween.TweenProperty(this, "position", stopStone.Position, dis / 400f);
                    movingTween.TweenCallback(Callable.From(() =>
                    {
                        currentStopStone = stopStone;
                    }));
                }
            }
        }
    }
}
