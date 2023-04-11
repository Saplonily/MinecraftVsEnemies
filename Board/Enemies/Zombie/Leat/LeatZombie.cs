using MVE.SalExt;

namespace MVE;

public partial class LeatZombie : Zombie
{
    [Export] protected Godot.Collections.Array<AudioStream> leatBeHitAudios = default!;
    protected Chooser<AudioStreamPlayer> leatBeHitAudioPlayerChooser = default!;
    protected Sprite2D leatCapSprite = default!;

    public double CapHp { get; set; } = 400d;
    public double CapMaxHp { get; set; } = 400d;

    public override void _Ready()
    {
        base._Ready();
        leatCapSprite = GetNode<Sprite2D>("Sprites/LeatCap");

        leatBeHitAudioPlayerChooser = SalAudioPool.GetChooserFromArray(leatBeHitAudios, (new(default!, Bus: "Board")));
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        CapHp = Math.Clamp(CapHp, 0d, CapMaxHp);
        leatCapSprite.Visible = CapHp > 0;
    }

    public override void BeHurt(double amount)
    {
        if (CapHp <= 0d)
        {
            base.BeHurt(amount);
        }
        else
        {
            CapHp -= amount;
            leatBeHitAudioPlayerChooser.Choose().Play();
            animationTree.Set("parameters/CapHitOneShot/request", (int)AnimationNodeOneShot.OneShotRequest.Fire);
        }
    }
}
