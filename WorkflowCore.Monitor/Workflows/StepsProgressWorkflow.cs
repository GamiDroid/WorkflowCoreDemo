using System.Collections.Concurrent;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace WorkflowCore.Monitor.Workflows;

public class StepsProgress
{
    public StepsProgress()
    {
        Steps["validate"] = PointerStatus.Pending;
        Steps["create_batch"] = PointerStatus.Pending;
        Steps["load_printer"] = PointerStatus.Pending;
        Steps["load_robot_params"] = PointerStatus.Pending;
        Steps["start_order"] = PointerStatus.Pending;
    }

    public ConcurrentDictionary<string, PointerStatus> Steps { get; set; } = [];

    public string? PrdNr { get; set; }
}

public class StepsProgressWorkflow : IWorkflow<StepsProgress>
{
    public string Id => nameof(StepsProgressWorkflow);
    public int Version => 1;

    public void Build(IWorkflowBuilder<StepsProgress> builder)
    {
        builder
            .StartWith(_ => ExecutionResult.Next()).Name("Start")

            .Then<ValidateStep>(b => b
                .Output((s, d) => d.Steps["validate"] = s.IsValid ? PointerStatus.Complete : PointerStatus.Failed)
            )

            .Then<CreateBatchStep>(b => b
                .CancelCondition(d => d.Steps["validate"] != PointerStatus.Complete)
                
                .Output((s, d) => d.Steps["create_batch"] = PointerStatus.Complete)
            )

            .Then<LoadPrinterStep>(b => b
                .Output((s, d) => d.Steps["load_printer"] = PointerStatus.Complete)
            )

            .Then<LoadRobotParametersStep>(b => b
                .Output((s, d) => d.Steps["load_robot_params"] = PointerStatus.Complete)
            )

            .Then<StartOrderStep>(b => b
                .Output((s, d) => d.Steps["start_order"] = PointerStatus.Complete)
            )

            .Then(_ => ExecutionResult.Next()).Name("End");
    }

    public class ValidateStep(IConfiguration config) : IStepBody
    {
        public bool IsValid => config.GetValue<bool>("StepsProgressWorkflow:Valid");

        public async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
        {
            await Task.Delay(3_000);
            return ExecutionResult.Next();
        }
    }

    public class CreateBatchStep : IStepBody
    {
        public async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
        {
            await Task.Delay(1_000);
            return ExecutionResult.Next();
        }
    }

    public class LoadPrinterStep : IStepBody
    {
        public async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
        {
            await Task.Delay(1_000);
            return ExecutionResult.Next();
        }
    }

    public class LoadRobotParametersStep : IStepBody
    {
        public async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
        {
            await Task.Delay(1_000);
            return ExecutionResult.Next();
        }
    }

    public class StartOrderStep : IStepBody
    {
        public async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
        {
            await Task.Delay(10_000);
            return ExecutionResult.Next();
        }
    }
}
