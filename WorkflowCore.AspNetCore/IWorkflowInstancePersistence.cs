using WorkflowCore.Models;

namespace WorkflowCore.AspNetCore;

public interface IWorkflowInstancePersistence
{
    Task PersistAsync(WorkflowInstance workflow, CancellationToken cancellationToken = default);
}