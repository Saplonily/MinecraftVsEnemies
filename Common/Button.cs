using MVE.SalExt;

namespace MVE;

public partial class Button : Godot.Button
{
    [Export] protected AudioStream clickAudio = default!;
    [Export] protected string bus = "Master";

    protected AudioStreamPlayer clickAudioPlayer = default!;


    public override void _Ready()
    {
        clickAudioPlayer = SalAudioPool.GetPlayer(new(clickAudio, Bus: bus));
    }

    public override void _Pressed()
    {
        clickAudioPlayer.Play();
    }
}
