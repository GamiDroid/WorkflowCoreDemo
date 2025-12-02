using System.Text.Json;
using System.Text.Json.Serialization;
using WorkflowCore.Models;

namespace WorkflowCore.AspNetCore.Json;

/// <summary>
/// JSON converter that serializes WorkflowStatus enum as string instead of number.
/// </summary>
public class WorkflowStatusJsonConverter : JsonConverter<WorkflowStatus>
{
    public override WorkflowStatus Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var stringValue = reader.GetString();
            if (Enum.TryParse<WorkflowStatus>(stringValue, true, out var result))
            {
                return result;
            }
            throw new JsonException($"Unable to convert \"{stringValue}\" to WorkflowStatus.");
        }
        else if (reader.TokenType == JsonTokenType.Number)
        {
            // Support reading from number for backwards compatibility
            var intValue = reader.GetInt32();
            if (Enum.IsDefined(typeof(WorkflowStatus), intValue))
            {
                return (WorkflowStatus)intValue;
            }
            throw new JsonException($"Unable to convert {intValue} to WorkflowStatus.");
        }

        throw new JsonException($"Unexpected token type {reader.TokenType} when parsing WorkflowStatus.");
    }

    public override void Write(Utf8JsonWriter writer, WorkflowStatus value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}
