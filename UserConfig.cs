using CursorShape = Godot.Control.CursorShape;

namespace MVE;

public class UserConfig
{
    public CursorShape CursorShapeReadyToPick => CursorShape.PointingHand;

    public CursorShape CursorShapeReadyToPickCard => CursorShape.PointingHand;

    public CursorShape CursorShapeReadyToPickPickaxe => CursorShape.PointingHand;
}
