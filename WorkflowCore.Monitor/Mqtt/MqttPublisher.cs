using MQTTnet;
using MQTTnet.Exceptions;
using System.Text.Json;

namespace WorkflowCore.Monitor.Mqtt;

public interface IMqttPublisher
{
    Task PublishAsync(string topic, object? message, bool retained = false, CancellationToken cancellationToken = default);
}

public class MqttPublisher(IMqttConnection mqtt) : IMqttPublisher
{
    private readonly IMqttConnection _mqtt = mqtt;

    public async Task PublishAsync(string topic, object? message, bool retained, CancellationToken cancellationToken)
    {
        var mqttMessageBuilder = new MqttApplicationMessageBuilder()
            .WithContentType("application/json")
            .WithTopic(topic)
            .WithRetainFlag(retained)
            .WithMessageExpiryInterval((uint)TimeSpan.FromDays(14).Seconds)
            .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce);

        if (message != null)
            mqttMessageBuilder = mqttMessageBuilder.WithPayload(JsonSerializer.Serialize(message, new JsonSerializerOptions(JsonSerializerDefaults.Web)));

        var mqttMessage = mqttMessageBuilder.Build();
        
        var client = _mqtt.GetClient();

        var result = await client.PublishAsync(mqttMessage, cancellationToken);
        if (!result.IsSuccess)
        {
            throw new MqttCommunicationException($"Failed to publish message to topic '{topic}'. Reason: {result.ReasonCode}");
        }
    }
}
