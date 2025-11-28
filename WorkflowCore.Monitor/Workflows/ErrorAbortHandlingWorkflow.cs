using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace WorkflowCore.Monitor.Workflows;

public class ErrorAbortHandlingWorkflow : IWorkflow
{
    public string Id => nameof(ErrorAbortHandlingWorkflow);
    public int Version => 1;

    public void Build(IWorkflowBuilder<object> builder)
    {
        builder
            .StartWith(ctx => ExecutionResult.Next()).Name("Start")
            // I use default error handling settings.
            // But the workflow will terminate on WorkflowAbortException.
            // Terminate will be done using WorkflowTerminateErrorHandler.
            .Then<ErrorProneStep>()
            .Then(ctx => ExecutionResult.Next()).Name("End");
    }

    private class ErrorProneStep : StepBody
    {
        public override ExecutionResult Run(IStepExecutionContext context)
        {
            throw new WorkflowAbortException("Abort workflow!");
        }
    }
}
