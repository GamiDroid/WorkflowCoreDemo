using System.Text.Json;
using System.Text.Json.Serialization;
using WorkflowCore.Models;

namespace WorkflowCore.AspNetCore.Json;

/// <summary>
/// JSON converter that serializes PointerStatus enum as string instead of number.
/// </summary>
public class PointerStatusJsonConverter : JsonConverter<PointerStatus>
{
    public override PointerStatus Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var stringValue = reader.GetString();
            if (Enum.TryParse<PointerStatus>(stringValue, true, out var result))
            {
                return result;
            }
            throw new JsonException($"Unable to convert \"{stringValue}\" to PointerStatus.");
        }
        else if (reader.TokenType == JsonTokenType.Number)
        {
            // Support reading from number for backwards compatibility
            var intValue = reader.GetInt32();
            if (Enum.IsDefined(typeof(PointerStatus), intValue))
            {
                return (PointerStatus)intValue;
            }
            throw new JsonException($"Unable to convert {intValue} to PointerStatus.");
        }

        throw new JsonException($"Unexpected token type {reader.TokenType} when parsing PointerStatus.");
    }

    public override void Write(Utf8JsonWriter writer, PointerStatus value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}
