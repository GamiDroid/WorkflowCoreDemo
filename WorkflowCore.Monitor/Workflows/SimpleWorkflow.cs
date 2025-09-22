using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace WorkflowCore.Monitor.Workflows;

public sealed class SimpleWorkflow : IWorkflow<SimpleWorkflowData>
{
    public string Id => nameof(SimpleWorkflow);
    public int Version => 1;

    public void Build(IWorkflowBuilder<SimpleWorkflowData> builder)
    {
        builder
            .StartWith(ctx => ExecutionResult.Next())
            .Delay(d => TimeSpan.FromSeconds(10))
            .Then<ConsoleWriteStep>(setup =>
            {
                setup.Id("ConsoleWriteStep#0");
            }).CancelCondition(d => d.IsStepExecuted("ConsoleWriteStep#0"), proceedAfterCancel: true)
            .Delay(d => TimeSpan.FromSeconds(25))
            .Then<ConsoleWriteStep>(setup =>
            {
                setup.Id("ConsoleWriteStep#1");
            }).CancelCondition(d => d.IsStepExecuted("ConsoleWriteStep#1"), proceedAfterCancel: true)
            .Delay(d => TimeSpan.FromSeconds(5))
            .EndWorkflow();
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

public class SimpleWorkflowData
{
    public string WorkflowId { get; set; } = Guid.NewGuid().ToString();
    public List<StepState> StepsExecuted { get; set; } = [];

    public bool IsStepExecuted(string key) => StepsExecuted.Any(s => s.Key == key);
}

public record StepState(string Key, DateTime Started);