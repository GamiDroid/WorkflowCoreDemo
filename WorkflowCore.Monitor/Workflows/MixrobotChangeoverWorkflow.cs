using WorkflowCore.AspNetCore.Extensions;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace WorkflowCore.Monitor.Workflows;

public class MixrobotChangeoverState
{
    public uint ProductionId { get; set; }
    public string? RouteNr { get; set; }
}

public partial class MixrobotChangeoverWorkflow : IWorkflow<MixrobotChangeoverState>
{
    public string Id => nameof(MixrobotChangeoverWorkflow);
    public int Version => 1;

    public void Build(IWorkflowBuilder<MixrobotChangeoverState> builder)
    {
        builder
            .Init()

            .Then<CreateBatchStep>(b => b
                .Name("Create batch")
                .OnError(WorkflowErrorHandling.Terminate)
            )

            .Then<CreateBatchWaitStep>(b => b
                .Name("Wait create batch")
            )

            .Then<StartOrderStep>(b => b
                .Name("Start production order")
            )

            .Then<StartOrderWaitStep>(b => b
                .Name("Wait start production order")
            )

            .Then<LoadBoxPrinterLabelStep>(b => b
                .Name("Load box printer label")
            )

            .Then<LoadBoxPrinterLabelWaitStep>(b => b
                .Name("Wait load box printer label")
            )

            .Parallel()
                .Do(b => b
                    .Then<PublishOrderInfoStep>(b => b
                        .Name("Publish order info")
                    )
                )
                .Do(b => b
                    .Then<PublishProductPalletInfoStep>(b => b
                        .Name("Publish product pallet info")
                    )
                )
                .Do(b => b
                    .Then<ResetNoReadSettingsStep>(b => b
                        .Name("Reset NoRead settings")
                    )
                )
                .Do(b => b
                    .Then<DownloadRecipeToPlcStep>(b => b
                        .Name("Download recipe to PLC")
                    )
                )
            .Join()

            .Then<FinishChangeoverStep>(b => b
                .Name("Finish changeover")
            )

            .Then(_ => ExecutionResult.Next()).Name("End");
    }
}
