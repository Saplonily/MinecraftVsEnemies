namespace MVE;

public partial class BluePrint : Drop
{
    protected AnimationPlayer animationPlayer = default!;

    public override void _Ready()
    {
        base._Ready();
        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
    }

#if GAME_DEBUG
    public override void _Process(double delta)
    {
        base._Process(delta);
        if (Input.IsKeyPressed(Key.K))
        {
            Free();
        }
    }
#endif

    public override async void OnPicking()
    {
        base.OnPicking();
        EnableGravity = false;
        Velocity = default;
        Vector2 target = GetViewport().GetCamera2D().GetScreenCenterPosition();
        target = Lawn.ToLocal(target);
        Vector3 target3 = target.ToVec3WithZ(40f);
        target3.Y += 40f;
        float distance = LevelPos.DistanceTo(target3);

        var tween = CreateTween()
            .SetTrans(Tween.TransitionType.Quad)
            .SetEase(Tween.EaseType.InOut);
        tween.TweenMethod(Callable.From<Vector3>(p => LevelPos = p), LevelPos, target3, Math.Max(distance / 200f, 0.5f));
        await ToSignal(tween, Tween.SignalName.Finished);
        animationPlayer.Play("Floating");

        await ToSignal(GetTree().CreateTimer(2), SceneTreeTimer.SignalName.Timeout);
        Board.StateMachine.TravelTo(Board.LevelState.Ending);
    }
}
