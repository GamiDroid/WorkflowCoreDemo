using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace WorkflowCore.AspNetCore;

public class WorkflowMonitorService(
    IWorkflowHost host,
    IWorkflowRegistry registry,
    IWorkflowRepository repository)
{
    private readonly IWorkflowHost _host = host;
    private readonly IWorkflowRegistry _registry = registry;
    private readonly IWorkflowRepository _repository = repository;

    public Task<IEnumerable<WorkflowInstance>> GetWorkflowInstancesAsync()
    {
#pragma warning disable CS0612 // Type or member is obsolete
        return _repository.GetWorkflowInstances(null, null, null, null, 0, 100);
#pragma warning restore CS0612 // Type or member is obsolete
    }

    public Task<WorkflowInstance> GetWorkflowInstanceAsync(string id)
    {
        return _repository.GetWorkflowInstance(id);
    }

    public IEnumerable<WorkflowDefinition> GetRegisteredWorkflows()
    {
        return _registry.GetAllDefinitions();
    }
}
