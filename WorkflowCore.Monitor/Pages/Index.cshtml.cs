using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WorkflowCore.Interface;
using WorkflowCore.Models;
using WorkflowCore.Monitor.Services;
using WorkflowCore.Monitor.Workflows;

namespace WorkflowCore.Monitor.Pages;

public class IndexModel(
    WorkflowMonitorService workflowMonitor,
    IWorkflowHost host) : PageModel
{
    private readonly WorkflowMonitorService _workflowMonitor = workflowMonitor;
    private readonly IWorkflowHost _host = host;

    public IEnumerable<WorkflowInstance> WorkflowInstances { get; private set; } = [];
    public IEnumerable<WorkflowDefinition> WorkflowDefinitions { get; private set; } = [];

    public async Task OnGetAsync()
    {
        WorkflowInstances = await _workflowMonitor.GetWorkflowInstances();
        WorkflowDefinitions = _workflowMonitor.GetRegisteredWorkflows();
    }

    public async Task<IActionResult> OnPostStartAsync(string id)
    {
        _ = await _host.StartWorkflow(id);

        return RedirectToPage();
    }
}