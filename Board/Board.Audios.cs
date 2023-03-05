using MVE.SalExt;

namespace MVE;

public partial class Board
{
#nullable disable
    [Export] protected Godot.Collections.Array<AudioStream> audioStones;
#nullable restore

    public Chooser<AudioStreamPlayer> StoneSoundPlayerChooser { get; set; } = null!;

    protected void InitAudios()
    {
        StoneSoundPlayerChooser = audioStones.GetChooser(new(null!, Bus: "Board"));
    }
}
