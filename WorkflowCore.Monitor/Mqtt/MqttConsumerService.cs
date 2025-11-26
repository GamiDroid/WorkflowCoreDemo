using MQTTnet;

namespace WorkflowCore.Monitor.Mqtt;

public interface IMqttConsumerService
{
    void AddConsumer<T>(string topic) where T : IMqttConsumer;
    Task HandleConsumerAsync(string topic, MqttApplicationMessageReceivedEventArgs message);
}

public class MqttConsumerService(ILogger<MqttConsumerService> logger, IServiceScopeFactory scopeFactory) : IMqttConsumerService
{
    private readonly ILogger<MqttConsumerService> _logger = logger;
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    
    private readonly Dictionary<string, Type> _consumers = [];

    public void AddConsumer<T>(string topic) where T : IMqttConsumer
    {
        if (_consumers.ContainsKey(topic))
            throw new InvalidOperationException($"A consumer for topic '{topic}' is already registered.");

        _consumers[topic] = typeof(T);
    }

    public Task HandleConsumerAsync(string topic, MqttApplicationMessageReceivedEventArgs message)
    {
        try
        {
            var matchingConsumerTopics = _consumers.Keys.Where(filter => MqttTopicFilterComparer.Compare(topic, filter) == MqttTopicFilterCompareResult.IsMatch);
            foreach (var matchedTopic in matchingConsumerTopics)
            {
                if (_consumers.TryGetValue(matchedTopic, out var consumerType))
                {
                    using var scope = _scopeFactory.CreateScope();
                    if (ActivatorUtilities.CreateInstance(scope.ServiceProvider, consumerType) is IMqttConsumer consumer)
                    {
                        _ = consumer.HandleAsync(message);
                    }
                    else
                    {
                        _logger.LogError("Failed to create instance of consumer type '{ConsumerType}' for topic '{Topic}'.", consumerType.FullName, topic);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Something went wrong when handling consumer.");
        }

        return Task.CompletedTask;
    }
}
