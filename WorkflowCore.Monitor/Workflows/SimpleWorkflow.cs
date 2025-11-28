using WorkflowCore.AspNetCore;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace WorkflowCore.Monitor.Workflows;

public sealed class SimpleWorkflow : IWorkflow
{
    public string Id => nameof(SimpleWorkflow);
    public int Version => 1;

    public void Build(IWorkflowBuilder<object> builder)
    {
        builder
            .StartWith(ctx => ExecutionResult.Next())
            .Delay(d => TimeSpan.FromSeconds(2))
            .Parallel()
            .Do(b => b
                .Then<ConsoleWriteStep>(setup => setup
                    .Id("ConsoleWriteStep#0")
                    .Input((s, d) => s.Delay = 2500)
                 )
                .Delay(d => TimeSpan.FromSeconds(10))
                .Then<ConsoleWriteStep>(setup => setup
                    .Id("ConsoleWriteStep#0")
                    .Input((s, d) => s.Delay = 1000)
                 )
            )
            .Join();
    }

    private class ConsoleWriteStep : StepBodyAsync
    {
        public int Delay { get; set; }

        public override async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
        {
            Console.WriteLine(DateTime.UtcNow);

            await Task.Delay(Delay);

            return ExecutionResult.Next();
        }
    }
}
