using MQTTnet;

namespace WorkflowCore.Monitor.Mqtt;

public static class MqttExtensions
{
    public static IServiceCollection AddMqtt(this IServiceCollection services)
    {
        services.AddSingleton<MqttClientFactory>();
        services.AddTransient<IMqttClient>(sp => sp.GetRequiredService<MqttClientFactory>().CreateMqttClient());

        services.AddSingleton<IMqttConnection, MqttConnection>();
        services.AddSingleton<IMqttPublisher, MqttPublisher>();
        services.AddSingleton<IMqttConsumerService, MqttConsumerService>();

        services.AddHostedService<MqttConnectionHost>();

        return services;
    }
}
