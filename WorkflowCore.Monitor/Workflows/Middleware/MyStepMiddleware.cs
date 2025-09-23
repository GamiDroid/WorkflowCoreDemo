using WorkflowCore.Interface;
using WorkflowCore.Models;
using WorkflowCore.Monitor.Mqtt;

namespace WorkflowCore.Monitor.Workflows.Middleware;

public class MyStepMiddleware(ILogger<MyStepMiddleware> logger, IMqttPublisher publisher) : IWorkflowStepMiddleware
{
    public async Task<ExecutionResult> HandleAsync(IStepExecutionContext context, IStepBody body, WorkflowStepDelegate next)
    {
        var workflow = context.Workflow;
        var step = context.Step;

        if (workflow.Data is BaseWorkflowData simpleData)
        {
            logger.LogInformation("Begin step {StepId}:{StepName} of workflow {WorkflowDefinitionId} ({WorkflowId})",
                step.Id, step.Name, workflow.WorkflowDefinitionId, workflow.Id);

            var workflowId = simpleData.WorkflowId;

            string? stepKey = null;
            if (!string.IsNullOrEmpty(step.ExternalId))
            {
                stepKey = step.ExternalId;
            }
            else if (!string.IsNullOrEmpty(step.Name))
            {
                stepKey = step.Name;
            }

            if (!string.IsNullOrEmpty(stepKey))
            {
                var stepState = new StepState(stepKey, DateTime.Now);
                simpleData.StepsExecuted.Add(stepState);

                await publisher.PublishAsync($"workflow/{workflow.WorkflowDefinitionId}/{workflowId}/data", workflow.Data, true);

                var result = await next();

                stepState.EndTime = DateTime.Now;

                await publisher.PublishAsync($"workflow/{workflow.WorkflowDefinitionId}/{workflowId}/data", workflow.Data, true);

                logger.LogInformation("Finished step {StepId}:{StepName} of workflow {WorkflowDefinitionId} ({WorkflowId})",
                    step.Id, step.Name, workflow.WorkflowDefinitionId, workflow.Id);

                return result;
            }
        }

        return await next();
    }
}
