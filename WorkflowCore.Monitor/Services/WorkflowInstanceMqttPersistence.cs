using Mqtt.AspNetCore;
using System.Text.Json;
using System.Text.Json.Serialization;
using WorkflowCore.AspNetCore;
using WorkflowCore.Models;

namespace WorkflowCore.Monitor.Services;

public class WorkflowInstanceMqttPersistence(IMqttPublisher mqttPublisher) : IWorkflowInstancePersistence
{
    private readonly IMqttPublisher _mqttPublisher = mqttPublisher;

    private const string c_activeStatusTopicName = "active";
    private const string c_finalStatusTopicName = "final";

    private static readonly TimeSpan s_expiryTimeActive = TimeSpan.FromHours(4); // 4 hours
    private static readonly TimeSpan s_expiryTimeFinal = TimeSpan.FromDays(4); // 4 days

    public async ValueTask PersistAsync(WorkflowInstance workflow, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(workflow.Reference))
            return;

        if (workflow.Status is not (WorkflowStatus.Complete or WorkflowStatus.Terminated))
        {
            await _mqttPublisher.PublishAsync(
                topic: GetTopic(workflow, c_activeStatusTopicName),
                message: workflow,
                retained: true,
                expiryTime: s_expiryTimeActive,
                jsonSerializerOptions: JsonOptions,
                cancellationToken: cancellationToken);
        }
        else
        {
            // Publish null to active topic to clear retained message
            await _mqttPublisher.PublishAsync(
                topic: GetTopic(workflow, c_activeStatusTopicName),
                message: null,
                retained: true,
                cancellationToken: cancellationToken);

            await _mqttPublisher.PublishAsync(
                topic: GetTopic(workflow, c_finalStatusTopicName),
                message: workflow,
                retained: true,
                expiryTime: s_expiryTimeFinal,
                jsonSerializerOptions: JsonOptions,
                cancellationToken: cancellationToken);
        }
    }

    private static string GetTopic(WorkflowInstance workflow, string status)
    {
        return $"workflows-core/{workflow.WorkflowDefinitionId}:v{workflow.Version}/{status}/{workflow.Reference[..8]}/instance";
    }

    public readonly static JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters =
            {
                new ObjectTypeDiscriminatorConverter()
            }
    };

    private sealed class ObjectTypeDiscriminatorConverter : JsonConverter<object>
    {
        private const string c_typePropertyName = "$type";

        public override bool CanConvert(Type typeToConvert)
        {
            // Only handle properties typed as 'object', not derived types
            return typeToConvert == typeof(object);
        }

        public override object? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }

            // Handle primitive types directly
            switch (reader.TokenType)
            {
                case JsonTokenType.String:
                    return reader.GetString();
                case JsonTokenType.Number:
                    if (reader.TryGetInt64(out var longValue))
                        return longValue;
                    return reader.GetDouble();
                case JsonTokenType.True:
                    return true;
                case JsonTokenType.False:
                    return false;
                case JsonTokenType.StartArray:
                    return JsonSerializer.Deserialize<JsonElement>(ref reader, options);
            }

            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException($"Unexpected token type: {reader.TokenType}");
            }

            using var jsonDoc = JsonDocument.ParseValue(ref reader);
            var root = jsonDoc.RootElement;

            // If no $type property, return as JsonElement
            if (!root.TryGetProperty(c_typePropertyName, out var typeElement))
            {
                return root.Clone();
            }

            var typeName = typeElement.GetString();
            if (string.IsNullOrEmpty(typeName))
            {
                return root.Clone();
            }

            var actualType = Type.GetType(typeName);
            if (actualType == null)
            {
                // If type cannot be resolved, return as JsonElement
                return root.Clone();
            }

            var json = root.GetRawText();
            return JsonSerializer.Deserialize(json, actualType, options);
        }

        public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
                return;
            }

            var actualType = value.GetType();

            // If it's literally just an empty 'object' instance, write empty object
            if (actualType == typeof(object))
            {
                writer.WriteNullValue();
                return;
            }

            // For primitive types, serialize directly without type discriminator
            if (actualType.IsPrimitive || actualType == typeof(string) || actualType == typeof(decimal))
            {
                JsonSerializer.Serialize(writer, value, actualType, options);
                return;
            }

            var typeAssemblyQualifiedName = actualType.AssemblyQualifiedName;

            writer.WriteStartObject();
            writer.WriteString(c_typePropertyName, typeAssemblyQualifiedName);

            var json = JsonSerializer.Serialize(value, actualType, options);
            using var doc = JsonDocument.Parse(json);

            foreach (var property in doc.RootElement.EnumerateObject())
            {
                property.WriteTo(writer);
            }

            writer.WriteEndObject();
        }
    }
}
