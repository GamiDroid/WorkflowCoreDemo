using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace WorkflowCore.Monitor.Workflows;

public partial class MixrobotChangeoverWorkflow
{
    private static MixrobotChangeoverState GetWokflowData(IStepExecutionContext ctx)
    {
        return (MixrobotChangeoverState)ctx.Workflow.Data;
    }

    public abstract class BaseChangeoverStep : IStepBody
    {
        protected MixrobotChangeoverState Data { get; private set; } = null!;
        protected int RetryCount { get; private set; }

        public async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
        {
            Data = GetWokflowData(context);
            RetryCount = context.ExecutionPointer.RetryCount;

            if (!Data.HasError)
                await RunAsync();

            return ExecutionResult.Next();
        }

        protected abstract Task RunAsync();
    }

    public class ResetChangeoverStateStep : BaseChangeoverStep
    {
        protected override async Task RunAsync()
        {
            Data.SetError(nameof(ResetChangeoverStateStep), "A random error occured");

            Console.WriteLine("Reset changeover state");
        }
    }

    public class CreateBatchStep : BaseChangeoverStep
    {
        protected override Task RunAsync()
        {
            if (RetryCount > 5)
                Data.SetError(nameof(CreateBatchStep), "Failed to create batch after multiple retries.");

            Console.WriteLine("Create Batch");
            return Task.CompletedTask;
        }
    }

    public class CreateBatchWaitStep : BaseChangeoverStep
    {
        protected override Task RunAsync()
        {
            Console.WriteLine("Wait create Batch");

            return Task.CompletedTask;
        }
    }

    public class StartOrderStep : BaseChangeoverStep
    {
        protected override Task RunAsync()
        {
            Console.WriteLine("Start order");
            Data.SetError(nameof(CreateBatchStep), "Failed to create batch after multiple retries.");
            //throw new NotImplementedException();

            return Task.CompletedTask;
        }
    }

    public class StartOrderWaitStep : BaseChangeoverStep
    {
        protected override Task RunAsync()
        {
            Console.WriteLine("Wait start order");
            return Task.CompletedTask;
        }
    }

    public class LoadBoxPrinterLabelStep : BaseChangeoverStep
    {
        protected override Task RunAsync()
        {
            Console.WriteLine("Load box printer label");
            return Task.CompletedTask;
        }
    }

    public class LoadBoxPrinterLabelWaitStep : BaseChangeoverStep
    {
        protected override Task RunAsync()
        {
            Console.WriteLine("Wait load box printer label");
            return Task.CompletedTask;
        }
    }

    public class PublishOrderInfoStep : BaseChangeoverStep
    {
        protected override Task RunAsync()
        {
            Console.WriteLine("Publish order info");
            return Task.CompletedTask;
        }
    }

    public class PublishProductPalletInfoStep : BaseChangeoverStep
    {
        protected override Task RunAsync()
        {
            Console.WriteLine("Publish product pallet info");
            return Task.CompletedTask;
        }
    }

    public class ResetNoReadSettingsStep : BaseChangeoverStep
    {
        protected override Task RunAsync()
        {
            Console.WriteLine("Reset NoRead settings");
            return Task.CompletedTask;
        }
    }

    public class DownloadRecipeToPlcStep : BaseChangeoverStep
    {
        protected override Task RunAsync()
        {
            Console.WriteLine("Download Recipe To Plc");
            return Task.CompletedTask;
        }
    }

    public class FinishChangeoverStep : BaseChangeoverStep
    {
        protected override Task RunAsync()
        {
            Console.WriteLine("Finish changeover");
            return Task.CompletedTask;
        }
    }

    public class ChangeoverErrorStep : IStepBody
    {
        public Task<ExecutionResult> RunAsync(IStepExecutionContext ctx)
        {
            var data = (MixrobotChangeoverState)ctx.Workflow.Data;

            data.SetError(nameof(ChangeoverErrorStep), "Unknown error occured");

            Console.WriteLine("changeover Stop due Error");
            return Task.FromResult(ExecutionResult.Next());
        }
    }
}
