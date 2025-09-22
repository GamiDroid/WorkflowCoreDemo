using WorkflowCore.Interface;
using WorkflowCore.Models;
using WorkflowCore.Monitor.Mqtt;

namespace WorkflowCore.Monitor.Workflows.Middleware;

public class MyExecuteWorkflowMiddleware(ILogger<MyExecuteWorkflowMiddleware> logger, IMqttPublisher publisher) : IWorkflowMiddleware
{
    public WorkflowMiddlewarePhase Phase => WorkflowMiddlewarePhase.ExecuteWorkflow;

    public async Task HandleAsync(WorkflowInstance workflow, WorkflowDelegate next)
    {
        logger.LogInformation("Execution {WorkflowDefinitionId}", workflow.WorkflowDefinitionId);

        var workflowId = workflow.Id;
        if (workflow.Data is SimpleWorkflowData simpleData)
        {
            workflowId = simpleData.WorkflowId;
        }

        await next();

        if (workflow.Status == WorkflowStatus.Complete)
        {
            await publisher.PublishAsync($"workflow/{workflow.WorkflowDefinitionId}/{workflowId}/data", null, true);
        }
        else
        {
            await publisher.PublishAsync($"workflow/{workflow.WorkflowDefinitionId}/{workflowId}/data", workflow.Data, true);
        }
    }
}
