using System.Runtime.InteropServices;
using System.Text.Json;


namespace MVE;

public partial class Init : Node
{
    [Export] protected PackedScene initGotoScene = default!;

    public override void _Ready()
    {
#if GODOT_WINDOWS
        Native.ComInit();
#endif
        CallDeferred(MethodName.Unload);

        SaveData sd = new();
        sd.OwnedCards.Add(new("MVE", "dispenser"));
        sd.SaveToUser("user://save.bin");

        SaveData sd2 = new();
        sd2.ReadFromUser("user://save.bin");
        Game.Logger.LogInfo("Test", sd2.OwnedCards[0].ToString());

    }

    public void Unload()
    {
        var s = initGotoScene.Instantiate();
        Game.Instance.ChangeSceneTo(s);
    }
}
