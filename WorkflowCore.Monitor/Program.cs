using MudBlazor.Services;
using WorkflowCore.AspNetCore;
using WorkflowCore.Monitor.Components;
using WorkflowCore.Monitor.Services;
using WorkflowCore.Monitor.Workflows;

var builder = WebApplication.CreateBuilder(args);

// Add MudBlazor services
builder.Services.AddMudServices();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddWorkflow(setup =>
{
    setup.AddScoped<WorkflowMonitorService>();
    setup.AddWorkflowStepsFromAssembly();
});

builder.Services.AddScoped<WorkflowInstanceService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.UseWorkflow(r =>
{
    r.RegisterWorkflow<SimpleWorkflow>();
    r.RegisterWorkflow<LongDelayWorkflow>();
    r.RegisterWorkflow<ErrorRetryHandlingWorkflow>();
    r.RegisterWorkflow<ErrorAbortHandlingWorkflow>();
    r.RegisterWorkflow<StepsProgressWorkflow, StepsProgress>();
});

app.Run();
