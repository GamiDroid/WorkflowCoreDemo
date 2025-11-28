using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WorkflowCore.AspNetCore;
using WorkflowCore.Interface;

namespace WorkflowCore.AspNetCore;

public static class WorkflowExtensions
{
    public static IServiceCollection AddWorkflow(this IServiceCollection services, Action<IServiceCollection> setupAction)
    {
        services.AddWorkflow();

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
