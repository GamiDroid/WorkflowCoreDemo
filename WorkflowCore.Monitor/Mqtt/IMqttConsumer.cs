using MQTTnet;
using System.Text.Json;
using WorkflowCore.Interface;
using WorkflowCore.Monitor.Workflows;

namespace WorkflowCore.Monitor.Mqtt;

public interface IMqttConsumer
{
    Task HandleAsync(MqttApplicationMessageReceivedEventArgs message);
}
