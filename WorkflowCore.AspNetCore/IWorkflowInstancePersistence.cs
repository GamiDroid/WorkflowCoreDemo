using WorkflowCore.Models;

namespace WorkflowCore.AspNetCore;

public interface IWorkflowInstancePersistence
{
    ValueTask PersistAsync(WorkflowInstance workflow, CancellationToken cancellationToken = default);
}