using MQTTnet;

namespace WorkflowCore.Monitor.Mqtt;

public interface IMqttConnection
{
    Task StartAsync(CancellationToken cancellationToken);
    Task StopAsync(CancellationToken cancellationToken);
    IMqttClient GetClient();
}

public class MqttConnection : IMqttConnection
{
    private readonly IMqttClient _mqtt;

    public MqttConnection(IMqttClient mqtt)
    {
        _mqtt = mqtt;

        _mqtt.ApplicationMessageReceivedAsync += OnApplicationMessageReceivedAsync;
        _mqtt.ConnectedAsync += OnConnectedAsync;
        _mqtt.DisconnectedAsync += OnDisconnectedAsync;
    }

    #region Mqtt Client Listeners
    private async Task OnDisconnectedAsync(MqttClientDisconnectedEventArgs args)
    {
    }

    private async Task OnConnectedAsync(MqttClientConnectedEventArgs args)
    {
    }

    private async Task OnApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs arg)
    {
    }
    #endregion

    public Task StartAsync(CancellationToken cancellationToken)
    {
        var options = new MqttClientOptionsBuilder()
            .WithClientId("WorkflowCore.Monitor")
            .WithTcpServer("localhost", 1883)
            .WithCleanSession(false)
            .Build();

        return _mqtt.ConnectAsync(options, cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return _mqtt.DisconnectAsync(
            MqttClientDisconnectOptionsReason.NormalDisconnection,
            cancellationToken: cancellationToken);
    }

    public IMqttClient GetClient() => _mqtt;
}
