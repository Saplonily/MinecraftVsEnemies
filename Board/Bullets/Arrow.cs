namespace MVE;

public partial class Arrow : Bullet
{
    public float LifeTime { get; set; } = 10.0f;

    public Vector3 Direction { get; set; } = Vector3.Right;

    public float Speed { get; set; } = 400.0f;

    public Arrow()
    {
        EnableGravity = false;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        if (LifeTime >= 0.0f)
        {
            LifeTime -= (float)delta;
            if (LifeTime <= 0.0f)
            {
                this.QueueFree();
            }
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        this.LevelPos += Direction * Speed * (float)delta;
    }

}
