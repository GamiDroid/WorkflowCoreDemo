using ConcurrentCollections;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace WorkflowCore.Monitor.Workflows;

public class WorkflowTerminateErrorHandler(
    ILogger<WorkflowTerminateErrorHandler> logger,
    IWorkflowHost host) : BackgroundService
{
    private readonly IWorkflowHost _host = host;

    private readonly ConcurrentHashSet<string> _workflowIdsToTerminate = [];

    private async void WorkflowHost_OnStepError(WorkflowInstance workflow, WorkflowStep step, Exception exception)
    {
        if (exception is WorkflowAbortException)
            _workflowIdsToTerminate.Add(workflow.Id);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _host.OnStepError += WorkflowHost_OnStepError;

        while (!stoppingToken.IsCancellationRequested)
        {
            foreach (var workflowId in _workflowIdsToTerminate)
            {
                try
                {
                    logger.LogInformation("Terminating workflow {WorkflowId} due to WorkflowAbortException.", workflowId);
                    var terminated = await _host.TerminateWorkflow(workflowId);

                    if (terminated)
                    {
                        logger.LogInformation("Workflow {WorkflowId} terminated successfully.", workflowId);
                        _workflowIdsToTerminate.TryRemove(workflowId);
                    }
                    else
                    {
                        logger.LogWarning("Failed to terminate workflow {WorkflowId}.", workflowId);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error terminating workflow {WorkflowId}.", workflowId);
                }
            }

            await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
        }

        _host.OnStepError -= WorkflowHost_OnStepError;
    }
}
