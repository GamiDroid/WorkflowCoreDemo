using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace WorkflowCore.AspNetCore;

internal class PersistWorkflowPostMiddleware(
    IWorkflowInstancePersistence persistence) : IWorkflowMiddleware
{
    private readonly IWorkflowInstancePersistence _persistence = persistence;

    public WorkflowMiddlewarePhase Phase => WorkflowMiddlewarePhase.PostWorkflow;

    public async Task HandleAsync(WorkflowInstance workflow, WorkflowDelegate next)
    {
        await next();
        
        await _persistence.PersistAsync(workflow);
    }
}
