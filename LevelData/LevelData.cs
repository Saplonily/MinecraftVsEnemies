using System.Text.Json;
using System.Text.Json.Nodes;

namespace MVE;

public class LevelData
{
    public static LevelData LoadFromJsonStream(Stream stream)
    {
        var node = JsonNode.Parse(stream);

        return new();
    }
}
