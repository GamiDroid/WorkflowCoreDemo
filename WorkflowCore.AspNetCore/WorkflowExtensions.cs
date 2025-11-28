using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WorkflowCore.AspNetCore;
using WorkflowCore.Interface;
using WorkflowCore.Monitor.Workflows;

namespace WorkflowCore.AspNetCore;

public static class WorkflowExtensions
{
    public static IServiceCollection AddWorkflow(this IServiceCollection services, Action<IServiceCollection> setupAction)
    {
        services.AddWorkflow();

        services.AddHostedService<WorkflowTerminateErrorHandler>();

        setupAction(services);

        return services;
    }

    public static void UseWorkflow(this IHost app, Action<IWorkflowController> registerAction)
    {
        var workflowHost = app.Services.GetRequiredService<IWorkflowHost>();

        registerAction(workflowHost);

        workflowHost.Start();
    }
}
