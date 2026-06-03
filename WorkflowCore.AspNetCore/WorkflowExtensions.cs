using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;
using WorkflowCore.AspNetCore;
using WorkflowCore.Interface;

namespace WorkflowCore.AspNetCore;

public static class WorkflowExtensions
{
    public static IServiceCollection AddWorkflow(this IServiceCollection services, Action<IServiceCollection> setupAction)
    {
        services.AddWorkflow();

        //services.AddWorkflowMiddleware<PersistWorkflowPostMiddleware>();
        //services.AddWorkflowStepMiddleware<PersistWorkflowStepMiddleware>();

        setupAction(services);

        return services;
    }

    public static void UseWorkflow(this IHost app, Action<IWorkflowController> registerAction)
    {
        var workflowHost = app.Services.GetRequiredService<IWorkflowHost>();

        registerAction(workflowHost);

        workflowHost.Start();
    }

    public static void AddWorkflowStepsFromAssembly(this IServiceCollection services, Assembly? assembly = null)
    {
        assembly ??= Assembly.GetCallingAssembly();

        var stepTypes = assembly.GetTypes()
            .Where(t => typeof(IStepBody).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

        foreach (var stepType in stepTypes)
        {
            services.AddTransient(stepType);
        }
    }
}
