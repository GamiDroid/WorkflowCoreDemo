using WorkflowCore.Interface;
using WorkflowCore.Models;
using WorkflowCore.Monitor.Mqtt;

namespace WorkflowCore.Monitor.Workflows.Middleware;

public class MyStepMiddleware(ILogger<MyStepMiddleware> logger, IMqttPublisher publisher) : IWorkflowStepMiddleware
{
    public async Task<ExecutionResult> HandleAsync(IStepExecutionContext context, IStepBody body, WorkflowStepDelegate next)
    {
        logger.LogInformation("Before executing step {StepId} of workflow {WorkflowDefinitionId}", context.Step.Name, context.Workflow.WorkflowDefinitionId);

        var workflow = context.Workflow;
        var step = context.Step;
        
        var workflowId = workflow.Id;
        if (workflow.Data is SimpleWorkflowData simpleData)
        {
            workflowId = simpleData.WorkflowId;

            if (!string.IsNullOrEmpty(step.ExternalId))
            {
                simpleData.StepsExecuted.Add(new StepState(step.ExternalId, DateTime.Now));
            }
            else if (!string.IsNullOrEmpty(step.Name))
            {
                simpleData.StepsExecuted.Add(new StepState(step.Name, DateTime.Now));
            }

            await publisher.PublishAsync($"workflow/{workflow.WorkflowDefinitionId}/{workflowId}/data", workflow.Data, true);
        }

        var result = await next();

        await publisher.PublishAsync($"workflow/{workflow.WorkflowDefinitionId}/{workflowId}/data", workflow.Data, true);

        logger.LogInformation("After executing step ({StepId}:{StepName}) of workflow {WorkflowDefinitionId}", 
            context.Step.Id, context.Step.Name, context.Workflow.WorkflowDefinitionId);
            
        return result;
    }
}
