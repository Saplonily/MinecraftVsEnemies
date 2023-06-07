using MVE.SalExt;

namespace MVE;

public partial class HeavyPlate : Weapon
{
    public const double WeightLimit = 80d;
    protected bool pressed = false;
    protected double totalWeight = 0d;

    [Export] protected AudioStream audioStreamPressed = default!;
    [Export] protected AudioStream audioStreamReleased = default!;
    protected AudioStreamPlayer audioPlayerPressed = default!;
    protected AudioStreamPlayer audioPlayerReleased = default!;

    public override void _Ready()
    {
        base._Ready();
        Tag.Add(WeaponTags.InGroundNotBeAttackable);
        audioPlayerPressed = SalAudioPool.GetPlayer(new(audioStreamPressed, Bus: "Board"));
        audioPlayerReleased = SalAudioPool.GetPlayer(new(audioStreamReleased, Bus: "Board"));
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        var areas = hitBoxArea.GetOverlappingAreas();
        List<Enemy> enemies = new(2);
        foreach (var a in areas)
            if (a.Owner is Enemy e) enemies.Add(e);
        totalWeight = enemies.Sum(e => e.Weight);
        if (totalWeight >= WeightLimit)
            Boom();
        bool enemyOn = enemies.Count is > 0;
        if (pressed is true && enemyOn is false)
        {
            audioPlayerReleased.Play();
        }
        else if (pressed is false && enemyOn is true)
        {
            audioPlayerPressed.Play();
        }
        pressed = enemyOn;
    }

    public void Boom()
    {
        // TODO nothing here but remove self
        Die(DeathReason.None);
    }
}