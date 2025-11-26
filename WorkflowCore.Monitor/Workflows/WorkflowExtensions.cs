using WorkflowCore.Interface;

namespace WorkflowCore.Monitor.Workflows;

public static class WorkflowExtensions
{
    public static IServiceCollection AddWorkflow(this IServiceCollection services, Action<IServiceCollection> setupAction)
    {
        services.AddWorkflow();

        setupAction(services);

        return services;
    }

    public static void UseWorkflow(this IHost app)
    {
        var workflowHost = app.Services.GetRequiredService<IWorkflowHost>();

        workflowHost.RegisterWorkflow<SimpleWorkflow, ChangeoverData>();

        workflowHost.Start();
    }
}
