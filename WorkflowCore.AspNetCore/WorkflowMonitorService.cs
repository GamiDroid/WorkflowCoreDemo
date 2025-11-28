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
        return _repository.GetWorkflowInstances(null, null, null, null, 0, 100);
    }

    public Task<WorkflowInstance> GetWorkflowInstanceAsync(string id)
    {
        return _repository.GetWorkflowInstance(id);
    }

    public IEnumerable<WorkflowDefinition> GetRegisteredWorkflows()
    {
        return _registry.GetAllDefinitions();
    }

    public Task<string> StartWorkflowAsync(string workflowDefinitionName, int version)
    {
        return _host.StartWorkflow(workflowDefinitionName, version);
    }
}
