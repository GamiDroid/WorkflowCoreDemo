using WorkflowCore.Interface;
using WorkflowCore.Models;
using WorkflowCore.Primitives;

namespace WorkflowCore.AspNetCore.Extensions;

public static class WorkflowBuilderExtensions
{
    /// <summary>
    /// Initializes the workflow by setting a unique reference identifier.
    /// </summary>
    /// <typeparam name="TData">The workflow data type.</typeparam>
    /// <param name="builder">The workflow builder instance.</param>
    /// <returns>The workflow builder for method chaining.</returns>
    public static IStepBuilder<TData, InlineStepBody> Init<TData>(this IWorkflowBuilder<TData> builder, Action<WorkflowInstance>? instanceSetter = null)
        where TData : class
    {
        return builder.StartWith(context =>
        {
            SetDefaults(context);

            instanceSetter?.Invoke(context.Workflow);

            return ExecutionResult.Next();
        }).Name("Init");
    }

    private static void SetDefaults(IStepExecutionContext context)
    {
        context.Workflow.Reference = Guid.NewGuid().ToString();
    }
}