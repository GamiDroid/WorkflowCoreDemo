using System.Collections.Concurrent;
using WorkflowCore.AspNetCore.Extensions;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace WorkflowCore.Monitor.Workflows;

// Recur wordt in parallel uitgevoerd,
// dus er is geen garantie dat de status van de batch creatie in de volgende iteratie al is bijgewerkt.
// Daarom gebruiken we een while loop die blijft controleren op de status van de batch creatie totdat deze is voltooid of er een timeout optreedt.

public class RecurDemoWorkflow : IWorkflow<RecurDemoWorkflowData>
{
    public string Id => nameof(RecurDemoWorkflow);
    public int Version => 1;

    public void Build(IWorkflowBuilder<RecurDemoWorkflowData> builder)
    {
        builder
            .Init(w => w.Description = "A workflow that demonstrates the recur step")
            .Then<CreateBatchStep>()
            .While(d => !(d.BatchCreated || d.BatchCreationTimeout)).Do(s => s
                .Then<PollBatchCreationStatusStep>().Name("Poll batch creation status")
                .Delay(d => TimeSpan.FromSeconds(5))
            )
            .Then<FinishBatchCreationStep>();
    }

    private class CreateBatchStep : IStepBody
    {
        public Task<ExecutionResult> RunAsync(IStepExecutionContext context)
        {
            BatchManager.StartBatchCreation(context.Workflow.Id);

            return Task.FromResult(ExecutionResult.Next());
        }
    }

    private class PollBatchCreationStatusStep : IStepBody
    {
        private static readonly TimeSpan s_timeout = TimeSpan.FromSeconds(20);

        public async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
        {
            var data = (RecurDemoWorkflowData)context.Workflow.Data;

            data.BatchWaitStartedUtc ??= DateTime.UtcNow;

            if (DateTime.UtcNow - data.BatchWaitStartedUtc > s_timeout)
            {
                data.BatchCreationTimeout = true;
            }

            if (BatchManager.IsBatchCreated(context.Workflow.Id))
            {
                data.BatchCreated = true;
            }

            return ExecutionResult.Next();
        }
    }

    private class FinishBatchCreationStep : IStepBody
    {
        public Task<ExecutionResult> RunAsync(IStepExecutionContext context)
        {
            var data = (RecurDemoWorkflowData)context.Workflow.Data;
            if (data.BatchCreated)
            {
                Console.WriteLine("Batch creation completed successfully.");
            }
            else if (data.BatchCreationTimeout)
            {
                Console.WriteLine("Batch creation failed due to timeout.");
            }
            else
            {
                Console.WriteLine("Batch creation status is unknown.");
            }

            return Task.FromResult(ExecutionResult.Next());
        }
    }
}

public class RecurDemoWorkflowData
{
    public DateTime? BatchWaitStartedUtc { get; set; }
    public bool BatchCreated { get; set; }
    public bool BatchCreationTimeout { get; set; }
}

public static class BatchManager
{
    private static ConcurrentDictionary<string, bool> _batchCreationStatus = [];

    public static void StartBatchCreation(string batchId)
    {
        Console.WriteLine($"Creating batch {batchId}...");

        _batchCreationStatus[batchId] = false;
    }

    public static bool IsBatchCreated(string batchId)
    {
        return _batchCreationStatus.TryGetValue(batchId, out bool isCreated) && isCreated;
    }

    public static void SimulateBatchCreation(string batchId)
    {
        Console.WriteLine($"Batch {batchId} created.");
        _batchCreationStatus[batchId] = true;
    }
}