using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace WorkflowCore.AspNetCore;

internal class PersistWorkflowStepMiddleware(
    IWorkflowInstancePersistence persistence) : IWorkflowStepMiddleware
{
    private readonly IWorkflowInstancePersistence _persistence = persistence;

    public async Task<ExecutionResult> HandleAsync(IStepExecutionContext context, IStepBody body, WorkflowStepDelegate next)
    {
        await _persistence.PersistAsync(context.Workflow);

        var result = await next();

        await _persistence.PersistAsync(context.Workflow);

        return result;
    }
}
