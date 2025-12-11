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

        if (workflow.Status is not (WorkflowStatus.Terminated or WorkflowStatus.Complete))
        {
            await _mqttPublisher.PublishAsync(
                topic: $"workflows-core/{workflow.WorkflowDefinitionId}:{workflow.Version}/active/{workflow.Reference[..8]}",
                message: workflow,
                retained: true,
                expiryTime: TimeSpan.FromMinutes(15),
                cancellationToken: cancellationToken);
        }
        else
        {
            await _mqttPublisher.PublishAsync(
                topic: $"workflows-core/{workflow.WorkflowDefinitionId}:{workflow.Version}/active/{workflow.Reference[..8]}",
                message: null,
                retained: true,
                cancellationToken: cancellationToken);

            await _mqttPublisher.PublishAsync(
                topic: $"workflows-core/{workflow.WorkflowDefinitionId}:{workflow.Version}/final/{workflow.Reference[..8]}",
                message: workflow,
                retained: true,
                expiryTime: TimeSpan.FromDays(14),
                cancellationToken: cancellationToken);
        }
    }
}
