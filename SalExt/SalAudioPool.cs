namespace MVE.SalExt;

public record SalAudioConfig(
    AudioStream Stream,
    float VolumnDb = 0f,
    float PitchScale = 1f,
    int MaxPolyPhony = 2,
    string Bus = "Master"
    );

public partial class SalAudioPool : Node
{
    public static SalAudioPool Instance { get; protected set; } = default!;

    public SalAudioPool()
    {
        Instance = this;
    }

    protected AudioStreamPlayer GetPlayerInternal(SalAudioConfig config)
    {
        var player = GetChildren().OfType<AudioStreamPlayer>().FirstOrDefault(p => IsConfiguredPlayer(p, config));
        if (player is null)
        {
            AudioStreamPlayer p = new()
            {
                Bus = config.Bus,
                Stream = config.Stream,
                VolumeDb = config.VolumnDb,
                PitchScale = config.PitchScale,
                MaxPolyphony = config.MaxPolyPhony
            };
            AddChild(p);
            return p;
        }
        else
        {
            return player;
        }
    }

    public static AudioStreamPlayer GetPlayer(SalAudioConfig config)
        => Instance.GetPlayerInternal(config);

    protected static bool IsConfiguredPlayer(AudioStreamPlayer player, SalAudioConfig config)
        => player.Bus == config.Bus
        && player.Stream == config.Stream
        && player.VolumeDb == config.VolumnDb
        && player.PitchScale == config.PitchScale
        && player.MaxPolyphony == config.MaxPolyPhony;

    public static Chooser<AudioStreamPlayer> GetChooserFromArray(Godot.Collections.Array<AudioStream> streamArray, SalAudioConfig baseConfig)
        => new(Random.Shared, streamArray.Select(s => GetPlayer(baseConfig with { Stream = s })));
}
