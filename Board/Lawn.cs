namespace MVE;

public partial class Lawn : Node2D
{
    protected Board board = default!;
    protected bool levelEntitiesSortDirty = false;

    [Export] public int Rows { get; set; } = 5;
    [Export] public int Column { get; set; } = 9;
    public PlantingArea PlantingArea { get; protected set; } = default!;

    public override void _Ready()
    {
        PlantingArea = GetNode<PlantingArea>("PlantingArea");

        board = this.FindParent<Board>()!;

        ChildEnteredTree += n => levelEntitiesSortDirty = n is BoardEntity;
    }

    public override void _Process(double delta)
    {
        //我也不知道这是怎么运作起来的
        if (levelEntitiesSortDirty)
        {
            var childrenArray = GetChildren();
            int ind = 0;
            var sortedWithIndex = childrenArray
                .OrderBy(n =>
                {
                    float v = n is BoardEntity entity ? entity.LevelPos.Y : float.MinValue;
                    if (n is Enemy) v += 0.1f;
                    if (n is Drop) v += 0.2f;
                    if (n is Bullet) v += 0.3f;
                    return v;
                })
                .Select(n => (node: n, index: ind++));
            foreach (var (node, index) in sortedWithIndex)
                if (node.GetIndex() != index)
                    MoveChild(node, index);

            levelEntitiesSortDirty = false;
        }
    }

    public BoardEntity AddBoardEntity(BoardEntity entity, Vector3 levelPosition)
    {
        entity.LevelPos = levelPosition;
        this.AddChild(entity);
        return entity;
    }
}
