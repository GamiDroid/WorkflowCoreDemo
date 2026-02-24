using WorkflowCore.AspNetCore.Extensions;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace WorkflowCore.Monitor.Workflows;

public class CancelStepsWorkflow : IWorkflow
{
    public string Id => nameof(CancelStepsWorkflow);
    public int Version => 1;

    public void Build(IWorkflowBuilder<object> builder)
    {
        builder
            .Init()
            .Then<StartStep>().Name("Start step")
            .Then<ExecuteStep>().Name("Execute step")
            .Then<EndStep>().Name("End step");
    }

    private class StartStep : IStepBody
    {
        public async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
        {
            Console.WriteLine("Start step...");

            await Task.Delay(2000);

            return ExecutionResult.Next();
        }
    }
    
    private class ExecuteStep : IStepBody
    {
        public async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
        {
            Console.WriteLine("execute step...");

            await Task.Delay(20_000);

            return ExecutionResult.Next();
        }
    }

    private class EndStep : IStepBody
    {
        public async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
        {
            Console.WriteLine("End step...");

            await Task.Delay(2000);

            return ExecutionResult.Next();
        }
    }
}
