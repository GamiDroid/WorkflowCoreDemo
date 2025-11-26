using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace WorkflowCore.Monitor.Workflows;

public sealed class SimpleWorkflow : IWorkflow<ChangeoverData>
{
    public string Id => nameof(SimpleWorkflow);
    public int Version => 1;

    public void Build(IWorkflowBuilder<ChangeoverData> builder)
    {
        builder
            .StartWith(ctx => ExecutionResult.Next())
            .Delay(d => TimeSpan.FromSeconds(2))
            .Parallel()
            .Do(b => b

                .Then<ConsoleWriteStep>(setup =>
                 {
                     setup.Id("ConsoleWriteStep#0");
                 }).CancelCondition(d => d.IsStepExecuted("ConsoleWriteStep#0"), proceedAfterCancel: true)
                .Delay(d => TimeSpan.FromSeconds(10))
                .Then<ConsoleWriteStep>(setup =>
                {
                    setup.Id("ConsoleWriteStep#1");
                }).CancelCondition(d => d.IsStepExecuted("ConsoleWriteStep#1"), proceedAfterCancel: true)
            )
            .Join();
    }

    private class ConsoleWriteStep : StepBodyAsync
    {
        public override async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
        {
            Console.WriteLine("#####################");

            await Task.Delay(5_000);

            return ExecutionResult.Next();
        }
    }
}

public class ChangeoverData : BaseWorkflowData
{
    public int ProductionId { get; set; }
    public string? BatchId { get; set; }
    public string? PrinterId { get; set; }
}
