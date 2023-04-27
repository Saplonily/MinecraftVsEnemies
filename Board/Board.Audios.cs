using MVE.SalExt;

namespace MVE;

public partial class Board : Node
{
    [Export, ExportGroup("Audios")] protected Godot.Collections.Array<AudioStream> audioStones = default!;

    public Chooser<AudioStreamPlayer> StoneSoundPlayerChooser { get; set; } = default!;

    protected void InitAudios()
    {
        StoneSoundPlayerChooser = SalAudioPool.GetChooserFromArray(audioStones, new(default!, Bus: "Board"));
    }
}
