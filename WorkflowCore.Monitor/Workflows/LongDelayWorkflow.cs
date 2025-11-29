using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace WorkflowCore.Monitor.Workflows;

public class LongDelayWorkflow : IWorkflow
{
    public string Id => nameof(LongDelayWorkflow);
    public int Version => 1;

    public void Build(IWorkflowBuilder<object> builder)
    {
        builder
            .StartWith(_ => ExecutionResult.Next()).Name("Start")
            .Delay(_ => TimeSpan.FromSeconds(20)).Name("Long Delay")
            .Then(_ => ExecutionResult.Next()).Name("End");
    }
}
