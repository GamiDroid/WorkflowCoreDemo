using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace WorkflowCore.Monitor.Services;

public class WorkflowMonitorService(
    IWorkflowRegistry registry,
    IWorkflowRepository repository)
{
    private readonly IWorkflowRegistry _registry = registry;
    private readonly IWorkflowRepository _repository = repository;

    public Task<IEnumerable<WorkflowInstance>> GetWorkflowInstances()
    {
        return _repository.GetWorkflowInstances(null, null, null, null, 0, 100);
    }

    public Task<WorkflowInstance> GetWorkflowInstance(string id)
    {
        return _repository.GetWorkflowInstance(id);
    }

    public IEnumerable<WorkflowDefinition> GetRegisteredWorkflows()
    {
        return _registry.GetAllDefinitions();
    }
}
