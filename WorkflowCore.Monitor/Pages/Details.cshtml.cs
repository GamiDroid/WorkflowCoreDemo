using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WorkflowCore.Models;
using WorkflowCore.Monitor.Services;

namespace WorkflowCore.Monitor.Pages;

public class DetailsModel(WorkflowMonitorService workflowMonitor) : PageModel
{
    private readonly WorkflowMonitorService _workflowMonitor = workflowMonitor;

    public WorkflowInstance Instance { get; private set; } = null!;

    public async Task<IActionResult> OnGetAsync(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return NotFound();
        }

        Instance = await _workflowMonitor.GetWorkflowInstance(id);

        if (Instance == null)
        {
            return NotFound();
        }

        return Page();
    }
}