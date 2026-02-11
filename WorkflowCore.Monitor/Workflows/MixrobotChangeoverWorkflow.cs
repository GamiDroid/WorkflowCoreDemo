using WorkflowCore.AspNetCore.Extensions;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace WorkflowCore.Monitor.Workflows;

public class MixrobotChangeoverState
{
    public uint ProductionId { get; set; }
    public string? RouteNr { get; set; }

    public bool HasError { get; private set; } = false;
    public string? ErrorMessage { get; private set; }
    public string? ErrorStep { get; private set; }

    public void SetError(string stepName, string message)
    {
        HasError = true;
        ErrorStep = stepName;
        ErrorMessage = message;
    }
}

public class ChangeoverStepInfo
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ChangeoverstepStatus Status { get; set; } = ChangeoverstepStatus.None;
}

public enum ChangeoverstepStatus
{
    None,
    Todo,
    Busy,
    Done,
    Error,
    Skipped,
}

public partial class MixrobotChangeoverWorkflow : IWorkflow<MixrobotChangeoverState>
{
    public string Id => nameof(MixrobotChangeoverWorkflow);
    public int Version => 1;

    public void Build(IWorkflowBuilder<MixrobotChangeoverState> builder)
    {
        builder
            .Init()

            .Saga(b => b

                .Then<ResetChangeoverStateStep>(b => b
                    .Name("Reset changeover state")
                )

                .Then<CreateBatchStep>(b => b
                    .Name("Create batch")
                    .OnError(WorkflowErrorHandling.Retry, TimeSpan.FromSeconds(5))
                    .CancelCondition(state => state.HasError)
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
            )
            .CompensateWith<ChangeoverErrorStep>()

            .Then(_ => ExecutionResult.Next()).Name("End");
    }
}
