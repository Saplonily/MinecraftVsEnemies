namespace MVE;

public partial class Init : Node
{
    [Export] public float TimeScale = 1.0f;
    [Export] protected PackedScene initGotoScene = default!;
    [Export] protected string? initGotoLevel;

    public override void _Ready()
    {
        //SaveData sd = new()
        //{
        //    OwnedCards =
        //    {
        //        "MVE/furnace"
        //    }
        //};
        //sd.SaveToUser("user://temp_test.bin");
        Engine.TimeScale = TimeScale;
#if GODOT_WINDOWS
        Native.ComInit();
#endif
        if (string.IsNullOrEmpty(initGotoLevel))
        {
            var s = initGotoScene.Instantiate();
            Game.Instance.ChangeSceneTo(s);
        }
        else
        {
            Game.Instance.SwitchToLevelResPath(initGotoLevel);
        }
    }
}
