using Mqtt.AspNetCore;
using WorkflowCore.AspNetCore;
using WorkflowCore.Models;

namespace WorkflowCore.Monitor.Services;

public class WorkflowInstanceMqttPersistence(IMqttPublisher mqttPublisher) : IWorkflowInstancePersistence
{
    private readonly IMqttPublisher _mqttPublisher = mqttPublisher;

    public Task PersistAsync(WorkflowInstance workflow, CancellationToken cancellationToken = default)
    {
        return _mqttPublisher.PublishAsync(
            topic: $"workflows-core/{workflow.WorkflowDefinitionId}:{workflow.Version}/{workflow.Id}", 
            message: workflow,
            retained: true,
            expiryTime: TimeSpan.FromMinutes(15),
            cancellationToken: cancellationToken);
    }
}
