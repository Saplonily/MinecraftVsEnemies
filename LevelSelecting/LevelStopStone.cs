namespace MVE.LevelSelecting;

public partial class LevelStopStone : StopStone
{
    public override bool AbleToEnter => true;
    [Export] public string LevelFile { get; protected set; } = default!;

    public override void OnEnter(PlayerHead playerHead)
    {
        Game.Instance.SwitchToLevelResPath(LevelFile);
    }
}
