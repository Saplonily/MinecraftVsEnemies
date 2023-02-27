namespace MVE;

public partial class Redstone : Drop
{
    public override void OnPicking()
    {
        base.OnPicking();
        var tween =
             CreateTween()
            .SetEase(Tween.EaseType.Out)
            .SetTrans(Tween.TransitionType.Cubic);
        if (Board.RequestDropPicking(this) == DropPickResult.ToRedstoneDisplayer)
        {

        }
    }
}
