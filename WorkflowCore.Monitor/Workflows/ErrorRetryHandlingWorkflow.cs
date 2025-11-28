using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace WorkflowCore.Monitor.Workflows;

public class ErrorRetryHandlingWorkflow : IWorkflow
{
    public string Id => nameof(ErrorRetryHandlingWorkflow);
    public int Version => 1;

    public void Build(IWorkflowBuilder<object> builder)
    {
        builder
            .StartWith(ctx => ExecutionResult.Next()).Name("Start")
            .Then<ErrorProneStep>(b => b
                // default error handling is retry with 60 seconds
                // WorkflowOptions.ErrorRetryInterval
                // WorkflowDefinition.ErrorRetryInterval
                // But these fields cannot be set.
                .OnError(WorkflowErrorHandling.Retry, TimeSpan.FromSeconds(2))
            )
            .Then(ctx => ExecutionResult.Next()).Name("End");
    }

    private class ErrorProneStep : StepBody
    {
        public override ExecutionResult Run(IStepExecutionContext context)
        {
            if (context.ExecutionPointer.RetryCount > 5)
                return ExecutionResult.Next();

            throw new Exception("Simulated error in workflow step.");
        }
    }
}
