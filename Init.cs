using HRESULT = System.Int32;

namespace MVE;

public partial class Init : Node
{
    [Export] public PackedScene MainScene = default!;
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
        Game.Logger.LogInfo("S", "What the fuck?");
        Game.Instance.MainScene = MainScene;
#if GODOT_WINDOWS
        HRESULT hr = Native.ComInit();
        if (hr is not >= 0)
        {
            _ = Native.NativeMessageBox("错误", "COM初始化失败");
        }
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
