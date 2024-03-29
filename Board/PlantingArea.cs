using System.Diagnostics;

namespace MVE;

public record struct PlantHintBox(bool Enabled, Rect2 LocalRegion, GodotColor Color)
{
    public Rect2I GridRegion
    {
        get
        {
            var pos = PlantingArea.LocalPosToGridPos(LocalRegion.Position);
            var size = PlantingArea.LocalPosToGridPos(LocalRegion.Size);
            return new(pos, size);
        }
    }
}

public partial class PlantingArea : Area2D
{
    public const float GridBoxSize = 75;
    protected PlantHintBox hintBox;
    protected Board board = default!;
    protected Lawn lawn = default!;
    protected Weapon?[,] gridWeapons = default!;

    [Export] public GodotColor PlaceAllowColor { get; set; }
    [Export] public GodotColor PlaceNotAllowColor { get; set; }

    public CollisionShape2D CollisionShape { get; set; } = default!;

    public Vector2I GridMousePosition => LocalPosToGridPos(GetLocalMousePosition());

    public override void _Ready()
    {
        base._Ready();
        board = this.FindParent<Board>()!;
        lawn = this.FindParent<Lawn>()!;
        CollisionShape = GetNode<CollisionShape2D>("CollisionShape2D");

        gridWeapons = new Weapon?[lawn.Column, lawn.Rows];
        InputEvent += PlantingArea_InputEvent;
        MouseExited += () =>
        {
            hintBox.Enabled = false;
            QueueRedraw();
        };

    }

    public override void _Draw()
    {
        base._Draw();
        if (hintBox.Enabled)
            DrawRect(hintBox.LocalRegion, hintBox.Color);
    }

    protected void PlantingArea_InputEvent(Node viewport, InputEvent ie, long shapeIdx)
    {
        if (ie.IsActionPressed(InputNames.Using))
            OnPlantInput();
        if (ie.IsActionPressed(InputNames.DebugPlace))
            OnDebugPlantInput();
        if (ie is InputEventMouseMotion iemm)
        {
            OnInputMotion(GetGlobalTransformWithCanvas().AffineInverse() * iemm.Position);
        }
    }

    protected void OnPlantInput()
    {
        if (board.Picking == PickingType.Card)
        {
            if (board.PickedNode is not Card card)
            {
                Game.Logger.LogError("Board", "LawnPlanting", "Board.Picking == PickingType.Card but with none-card PickedNode.");
                return;
            }
            if (!hintBox.Enabled && TryGetWeaponAt(GetLocalMousePosition(), out _))
            {
                Game.Logger.LogError("Board", "LawnPlanting", "hintBox not enabled");
                return;
            }
            var prop = card.WeaponProperty;
            var gridPos = hintBox.GridRegion.Position;

            if (TryPlantAt(gridPos, prop, out _))
            {
                card.OnUsed();
            }
        }
    }

    [Conditional("GAME_DEBUG")]
    protected void OnDebugPlantInput()
    {
        if (Input.IsKeyPressed(Key.Z))
            PlaceEnemyAt(GridMousePosition, Game.Instance.EnemyProperties["zombie"]);
        if (Input.IsKeyPressed(Key.A))
        {
            var award = GD.Load<PackedScene>("res://Board/Drop/BluePrint.tscn").Instantiate<BluePrint>();
            board.Lawn.AddBoardEntity(award, lawn.GetLocalMousePosition().ToVec3WithZ0());
            award.Velocity += new Vector3(board.Random.Next1m1Single(100, 200), 0, board.Random.NextSingle(100, 200));
        }
    }

    protected void OnInputMotion(Vector2 localPosition)
    {
        if (board.Picking is PickingType.Card)
        {
            var alinedPos = LocalPosAlineToGrid(localPosition);

            hintBox.Enabled = true;
            hintBox.LocalRegion = new Rect2(alinedPos, new(GridBoxSize, GridBoxSize));
            if (TryGetWeaponAt(localPosition, out var weaponAt))
            {
                if (weaponAt is null)
                    hintBox.Color = PlaceAllowColor;
                else
                    hintBox.Color = PlaceNotAllowColor;
            }
            else
            {
                hintBox.Enabled = false;
                QueueRedraw();
            }
            QueueRedraw();
        }
        else
        {
            if (hintBox.Enabled)
            {
                hintBox.Enabled = false;
                QueueRedraw();
            }
        }
    }

    public bool TryPlantAt(Vector2I gridPosition, WeaponProperty weaponProp, out Weapon? weapon)
    {
        if (TryGetWeaponAt(gridPosition, out Weapon? weaponAt) && weaponAt is null)
        {
            var ins = weaponProp.Scene.Instantiate<Weapon>();
            Vector2 localPos = new(GridBoxSize * (gridPosition.X + .5f), GridBoxSize * (gridPosition.Y + 1));
            lawn.AddBoardEntity(ins, (localPos + Position).ToVec3WithZ0());

            gridWeapons[gridPosition.X, gridPosition.Y] = ins;
            weapon = ins;
            ins.Died += OnWeaponDied;
            void OnWeaponDied(Weapon wea, DeathReason _)
            {
                gridWeapons[gridPosition.X, gridPosition.Y] = null;
                wea.Died -= OnWeaponDied;
            }
            weapon.OnPlaced(this);
            return true;
        }
        weapon = null;
        return false;
    }

    public bool TryGetWeaponAt(Vector2I gridPosition, out Weapon? weapon)
    {
        if (gridPosition.X >= 0 && gridPosition.X < lawn.Column && gridPosition.Y >= 0 && gridPosition.Y < lawn.Rows)
        {
            weapon = gridWeapons[gridPosition.X, gridPosition.Y];
            return true;
        }
        weapon = null;
        return false;
    }

    public bool TryGetWeaponAt(Vector2 localPosition, out Weapon? weapon)
        => TryGetWeaponAt(LocalPosToGridPos(localPosition), out weapon);

    public Enemy PlaceEnemyAt(Vector2I gridPosition, EnemyProperty enemy)
    {
        var ins = enemy.Scene.Instantiate<Enemy>();
        Vector2 localPos = new(GridBoxSize * (gridPosition.X + .5f), GridBoxSize * (gridPosition.Y + 1));
        lawn.AddBoardEntity(ins, (localPos + Position).ToVec3WithZ0());
        return ins;
    }

    public static Vector2I LocalPosToGridPos(Vector2 localPosition)
    {
        int x = (int)MathF.Floor(localPosition.X / GridBoxSize);
        int y = (int)MathF.Floor(localPosition.Y / GridBoxSize);
        return new(x, y);
    }

    public static Vector2 LocalPosAlineToGrid(Vector2 localPosition)
    {
        Vector2 p;
        p.X = Mathf.Floor(localPosition.X / GridBoxSize) * GridBoxSize;
        p.Y = Mathf.Floor(localPosition.Y / GridBoxSize) * GridBoxSize;
        return p;
    }

}
