using Godot;

namespace MVE;

public record struct PlantHintBox(bool Enabled, Rect2 LocalRegion, Color Color)
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
    protected Board board = null!;
    protected Lawn lawn = null!;
    protected Weapon?[,] gridWeapons = null!;

    [Export]
    public Color PlaceAllowColor { get; set; }
    [Export]
    public Color PlaceNotAllowColor { get; set; }

    public Vector2I GridMousePosition => LocalPosToGridPos(GetLocalMousePosition());

    public override void _Ready()
    {
        base._Ready();
        board = this.FindParent<Board>()!;
        lawn = this.FindParent<Lawn>()!;
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
        if (ie.IsActionPressed(InputNames.Plant))
            OnPlantInput();
        if (ie.IsActionPressed(InputNames.DebugPlace))
            OnDebugPlantInput();
        if (ie is InputEventMouseMotion iemm)
            OnInputMotion(this.GetGlobalTransformWithCanvas().AffineInverse() * iemm.Position);
    }

    protected void OnPlantInput()
    {
        if (board.Picking == PickingType.Card)
        {
            if (board.PickedCard == null)
            {
                Game.Logger.LogError("Board", "LawnPlanting", "Board.Picking == PickingType.Card but with null PickedCard.");
                return;
            }
            if (!hintBox.Enabled)
            {
                Game.Logger.LogError("Board", "LawnPlanting", "hintBox not enabled");
                return;
            }
            var prop = board.PickedCard.WeaponProperty;
            var gridPos = hintBox.GridRegion.Position;

            if (TryPlantAt(gridPos, prop, out var weapon))
            {
                board.PickedCard.OnUsed();
            }
        }
    }

    protected void OnDebugPlantInput()
    {
        this.PlaceEnemyAt(GridMousePosition, Game.Instance.EnemyProperties[0]);
    }

    protected void OnInputMotion(Vector2 localPosition)
    {
        if (board.Picking is PickingType.Card or PickingType.Pickaxe)
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
        if (TryGetWeaponAt(gridPosition, out Weapon? weaponAt))
        {
            if (weaponAt is null)
            {
                var ins = weaponProp.Scene.Instantiate<Weapon>();
                Vector2 localPos = new(GridBoxSize * (gridPosition.X + .5f), GridBoxSize * (gridPosition.Y + 1));
                lawn.AddBoardEntity(ins, (localPos + Position).ToVec3WithZ0());

                gridWeapons[gridPosition.X, gridPosition.Y] = ins;
                weapon = ins;
                weapon!.OnDestroyed += () =>
                {
                    gridWeapons[gridPosition.X, gridPosition.Y] = null;
                };
                return true;
            }
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