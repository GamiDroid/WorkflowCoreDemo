using WorkflowCore.Interface;
using WorkflowCore.Models;
using WorkflowCore.Monitor.Mqtt;

namespace WorkflowCore.Monitor.Workflows.Middleware;

public class MyPreWorkflowMiddleware(
    ILogger<MyPreWorkflowMiddleware> logger,
    IMqttPublisher publisher) : IWorkflowMiddleware
{
    public WorkflowMiddlewarePhase Phase { get; } = WorkflowMiddlewarePhase.PreWorkflow;

    public async Task HandleAsync(WorkflowInstance workflow, WorkflowDelegate next)
    {
        if (workflow.Data is BaseWorkflowData workflowData)
        {
            var workflowId = workflowData.WorkflowId;

            logger.LogInformation("Workflow {WorkflowDefinitionId} ({WorkflowId}) started", workflow.WorkflowDefinitionId, workflowId);

            await publisher.PublishAsync($"workflow/{workflow.WorkflowDefinitionId}/{workflowId}/data", workflow.Data, true);
        }

        await next();
    }
}

/// <summary>
/// Somehow when <see cref="IStepBuilder{TData, TStepBody}.EndWorkflow"> is used, PostWorkflow middleware is not executed.
/// </summary>
public class MyPostWorkflowMiddleware(
    ILogger<MyPostWorkflowMiddleware> logger,
    IMqttPublisher publisher) : IWorkflowMiddleware
{
    public WorkflowMiddlewarePhase Phase { get; } = WorkflowMiddlewarePhase.PostWorkflow;

    public async Task HandleAsync(WorkflowInstance workflow, WorkflowDelegate next)
    {
        if (workflow.Data is BaseWorkflowData workflowData)
        {
            var workflowId = workflowData.WorkflowId;

            logger.LogInformation("Workflow {WorkflowDefinitionId} ({WorkflowId}) finished", workflow.WorkflowDefinitionId, workflowId);

            workflowData.EndTime = DateTime.Now;

            await publisher.PublishAsync($"workflow/{workflow.WorkflowDefinitionId}/{workflowId}/data", workflow.Data, true, expiryTime: TimeSpan.FromSeconds(60));
        }

        await next();
    }
}
