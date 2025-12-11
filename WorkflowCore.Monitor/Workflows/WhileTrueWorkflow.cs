using WorkflowCore.AspNetCore.Extensions;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace WorkflowCore.Monitor.Workflows;

public class WhileTrueWorkflow : IWorkflow
{
    public string Id => nameof(WhileTrueWorkflow);
    public int Version => 1;

    public void Build(IWorkflowBuilder<object> builder)
    {
        builder
            .Init()
            .While((d, c) => true).Do(s => s
                .Then<FetchStatus>()
                .Delay(d => TimeSpan.FromSeconds(0.5)).Name("Delay 0.5sec")
            ).Name("While")
            .Then(_ => ExecutionResult.Next()).Name("End");
    }

    // !! Caution !!
    // Using while inside a step cannot be canceled by my knowledge.
    public class WhileTrueStep : IStepBody
    {
        public async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
        {
            while(!context.CancellationToken.IsCancellationRequested)
            {
                Console.WriteLine("Do something in while true loop");

                await Task.Delay(2500);
            }

            return ExecutionResult.Next();
        }
    }

    public class FetchStatus : IStepBody
    {
        public async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
        {
            return ExecutionResult.Next();
        }
    }
}
