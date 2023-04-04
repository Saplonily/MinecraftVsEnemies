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
        StoneSoundPlayerChooser = SalAudioPool.ChooserFromArray(audioStones, new(default!, Bus: "Board"));
    }
}
