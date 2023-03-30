using System.Diagnostics.CodeAnalysis;

namespace MVE;

public interface IBoardUIPickable
{
    PickShownConfig GetShownConfig();

    void OnPicked(PickingType source, PickingTravelType travelType);

    void OnUnpicked(PickingType source, PickingTravelType travelType);
}

public class PickShownConfig
{
    public required Texture2D Texture { get; set; }
    public required Transform2D Transform { get; set; } = Transform2D.Identity;
    public bool Centered { get; set; } = true;
    public Vector2 Offset { get; set; }
    public bool FlipH { get; set; } = false;
    public bool FlipV { get; set; } = false;

    [SetsRequiredMembers]
    public PickShownConfig(Sprite2D sprite)
    {
        Transform = sprite.Transform;
        Centered = sprite.Centered;
        Texture = sprite.Texture;
        Offset = sprite.Offset;
        FlipH = sprite.FlipH;
        FlipV = sprite.FlipV;
    }

    public PickShownConfig()
    {

    }

    public void ApplyToSprite(Sprite2D sprite)
    {
        sprite.Transform = Transform;
        sprite.Centered = Centered;
        sprite.Texture = Texture;
        sprite.Offset = Offset;
        sprite.FlipH = FlipH;
        sprite.FlipV = FlipV;
    }
}
