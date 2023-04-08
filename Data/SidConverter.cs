using System.Text.Json;
using System.Text.Json.Serialization;

namespace MVE;

public class SidConverter : JsonConverter<Sid>
{
    public override Sid Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string? str = reader.GetString() 
            ?? throw new JsonException("The string of the Sid string-formed one cannot be null.");
        return Sid.Parse(str);
    }

    public override void Write(Utf8JsonWriter writer, Sid value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}
