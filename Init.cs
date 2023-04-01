using System.Text.Json;

namespace MVE;

public partial class Init : Node
{
    [Export] protected PackedScene initGotoScene = default!;

    public override void _Ready()
    {
        CallDeferred(MethodName.Unload);
    }

    public void Unload()
    {
        var s = initGotoScene.Instantiate();
        Game.Instance.ChangeSceneTo(s);
    }
}
