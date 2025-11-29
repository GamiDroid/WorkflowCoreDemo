using WorkflowCore.Models;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace WorkflowCore.Monitor;
#pragma warning restore IDE0130 // Namespace does not match folder structure

public static class WorkflowUiHelper
{
    private const int s_defaultGuidStringLength = 8;

    public static string Display(this WorkflowInstance wi)
    {
        return WorkflowInstanceDisplay(wi.WorkflowDefinitionId, wi.Version, wi.Id);
    }

    public static string WorkflowInstanceDisplay(string workflowDefId, int version, string workflowInstanceId)
    {
        return $"{workflowDefId}:v{version} ({workflowInstanceId[..s_defaultGuidStringLength]})";
    }
}
