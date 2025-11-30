using Microsoft.Extensions.Logging;
using MQTTnet;

namespace Mqtt.AspNetCore;

public interface IMqttConnection
{
    Task StartAsync(CancellationToken cancellationToken);
    Task StopAsync(CancellationToken cancellationToken);
    Task AddConsumerAsync<T>(string topic) where T : IMqttConsumer;
    IMqttClient GetClient();
}

public class MqttConnection : IMqttConnection
{
    private readonly ILogger<MqttConnection> _logger;
    private readonly IMqttClient _mqtt;
    private readonly IMqttConsumerService _consumerService;

    public MqttConnection(ILogger<MqttConnection> logger, IMqttClient mqtt, IMqttConsumerService consumerService)
    {
        _logger = logger;
        _mqtt = mqtt;
        _consumerService = consumerService;

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
        foreach (var (topic, _) in _consumerService.GetConsumers())
        {
            var subscribeOptions = new MqttClientSubscribeOptionsBuilder()
                .WithTopicFilter(topic)
                .Build();

            await _mqtt.SubscribeAsync(subscribeOptions);
        }
    }

    private Task OnApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs arg)
    {
        var topic = arg.ApplicationMessage.Topic;
        _ = _consumerService.HandleConsumerAsync(topic, arg);

        return Task.CompletedTask;
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

    public async Task AddConsumerAsync<T>(string topic) where T : IMqttConsumer
    {
        _consumerService.AddConsumer<T>(topic);

        if (!_mqtt.IsConnected)
        {
            return;
        }

        var subscribeOptions = new MqttClientSubscribeOptionsBuilder()
            .WithTopicFilter(topic)
            .Build();

        await _mqtt.SubscribeAsync(subscribeOptions);

        _logger.LogInformation("Subscribed to topic '{Topic}' for consumer '{ConsumerType}'", topic, typeof(T).FullName);
    }
}
