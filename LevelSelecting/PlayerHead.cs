using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Godot;

namespace MVE.LevelSelecting;

public partial class PlayerHead : Node2D
{
    protected StopStone currentStopStone = null!;
    protected Tween? movingTween;
    protected RayCast2D rayCast = null!;
    protected AnimationPlayer animationPlayer = null!;

    protected bool noControl = false;

    public override void _Ready()
    {
        rayCast = GetNode<RayCast2D>("RayCast2D");
        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
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

    public override async void _Input(InputEvent ie)
    {
        if (currentStopStone.AbleToEnter && ie.IsActionPressed(InputNames.Accept))
        {
            noControl = true;
            animationPlayer.Play("Enter");
            await ToSignal(animationPlayer, AnimationPlayer.SignalName.AnimationFinished);
            currentStopStone.OnEnter(this);
        }
        if (currentStopStone is LevelStopStone && ie is InputEventKey iek && iek.Keycode == Key.Key1 && iek.Pressed)
        {
            unsafe
            {
                Native.COMDLG_FILTERSPEC f = new()
                {
                    pszName = "Level file (json)",
                    pszSpec = "*.json"
                };
                char* c = Native.OpenDialog(ref f, 1);
                if (c == null)
                {
                    Game.Logger.LogInfo("Test", "Cancelled");
                    return;
                }
                string result = new(c);
                Native.ComFree(c);
                Game.Instance.SwitchToLevelNativePath(result);
            }
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (noControl) return;
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
