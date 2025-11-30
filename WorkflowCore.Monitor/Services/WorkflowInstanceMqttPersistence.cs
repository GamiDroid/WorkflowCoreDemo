using Mqtt.AspNetCore;
using WorkflowCore.AspNetCore;
using WorkflowCore.Models;

namespace WorkflowCore.Monitor.Services;

public class WorkflowInstanceMqttPersistence(IMqttPublisher mqttPublisher) : IWorkflowInstancePersistence
{
    private readonly IMqttPublisher _mqttPublisher = mqttPublisher;

    public async ValueTask PersistAsync(WorkflowInstance workflow, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(workflow.Reference))
            return;

        await _mqttPublisher.PublishAsync(
            topic: $"workflows-core/{workflow.WorkflowDefinitionId}:{workflow.Version}/{workflow.Reference[..8]}", 
            message: workflow,
            retained: true,
            expiryTime: TimeSpan.FromMinutes(15),
            cancellationToken: cancellationToken);
    }
}
