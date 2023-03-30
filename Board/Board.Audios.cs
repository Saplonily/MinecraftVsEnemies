using MVE.SalExt;

namespace MVE;

public partial class Board : Node
{
#nullable disable
    [Export] protected Godot.Collections.Array<AudioStream> audioStones;
#nullable restore

    public Chooser<AudioStreamPlayer> StoneSoundPlayerChooser { get; set; } = default!;

    protected void InitAudios()
    {
        StoneSoundPlayerChooser = audioStones.GetChooser(new(default!, Bus: "Board"));
    }
}
