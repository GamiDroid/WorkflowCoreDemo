namespace Mqtt.AspNetCore;

public class MqttConnectionHost(IMqttConnection mqtt) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await mqtt.StartAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return mqtt.StopAsync(cancellationToken);
    }
}
