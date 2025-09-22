
namespace WorkflowCore.Monitor.Mqtt;

public class MqttConnectionHost(IMqttConnection mqtt) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        return mqtt.StartAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return mqtt.StopAsync(cancellationToken);
    }
}
