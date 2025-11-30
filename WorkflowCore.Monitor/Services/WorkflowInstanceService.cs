using MudBlazor;
using WorkflowCore.Interface;
using WorkflowCore.Models;
using WorkflowCore.Monitor.Components.Dialogs;

namespace WorkflowCore.Monitor.Services;

public class WorkflowInstanceService(
        IWorkflowHost host,
        ISnackbar snackbar,
        IDialogService dialogService
    )
{
    private readonly IWorkflowHost _host = host;
    private readonly ISnackbar _snackbar = snackbar;
    private readonly IDialogService _dialogService = dialogService;

    public async Task StartAsync(string defId, int version)
    {
        object? obj = null;

        if (defId == "StepsProgressWorkflow" && version == 1)
        {
            var options = new DialogOptions { CloseOnEscapeKey = true };

            var wData = new Workflows.StepsProgress();
            var parameters = new DialogParameters
            {
                ["Arguments"] = wData
            };

            var dRef = await _dialogService.ShowAsync<WorkflowArgsDialog>("Arguments", parameters, options);
            var dResult = await dRef.Result;

            if (dResult!.Canceled)
            {
                return;
            }

            obj = dResult.Data;
        }

        var workflowId =
            await _host.StartWorkflow(defId, version, obj);
        _snackbar.Add($"Worflow {WorkflowUiHelper.WorkflowInstanceDisplay(defId, version, workflowId)} started.", Severity.Success);
    }

    public async Task<bool> StopAsync(WorkflowInstance instance)
    {
        var terminated = await _host.TerminateWorkflow(instance.Id);

        if (!terminated)
        {
            _snackbar.Add($"Worflow {WorkflowUiHelper.WorkflowInstanceDisplay(instance.WorkflowDefinitionId, instance.Version, instance.Id)} coult not be terminated.", Severity.Error);
            return false;
        }

        _snackbar.Add($"Worflow {WorkflowUiHelper.WorkflowInstanceDisplay(instance.WorkflowDefinitionId, instance.Version, instance.Id)} terminated.", Severity.Warning);
        return true;
    }
}
