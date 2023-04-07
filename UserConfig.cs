using CursorShape = Godot.DisplayServer.CursorShape;

namespace MVE;

public class UserConfig
{
    public CursorShape CursorShapeReadyToPick { get; set; } = CursorShape.PointingHand;

    public CursorShape CursorShapeReadyToPickCard { get; set; } = CursorShape.PointingHand;

    public CursorShape CursorShapeReadyToPickPickaxe { get; set; } = CursorShape.PointingHand;
}
