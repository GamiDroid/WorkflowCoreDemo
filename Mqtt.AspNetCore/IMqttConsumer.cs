using MQTTnet;

namespace Mqtt.AspNetCore;

public interface IMqttConsumer
{
    Task HandleAsync(MqttApplicationMessageReceivedEventArgs message);
}
