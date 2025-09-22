using WorkflowCore.Interface;
using WorkflowCore.Monitor.Mqtt;
using WorkflowCore.Monitor.Services;
using WorkflowCore.Monitor.Workflows;
using WorkflowCore.Monitor.Workflows.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddWorkflow(sp =>
{
    sp.AddWorkflowMiddleware<MyExecuteWorkflowMiddleware>();
    sp.AddWorkflowStepMiddleware<MyStepMiddleware>();
});

// Add this line after your workflow-core registration
builder.Services.AddScoped<WorkflowMonitorService>();

builder.Services.AddMqtt();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapRazorPages();

app.UseWorkflow();

app.MapPost("/workflows/simple/start", async (IWorkflowHost host) =>
{
    var workflowId = await host.StartWorkflow(nameof(SimpleWorkflow), new SimpleWorkflowData());

    return Results.Ok($"SimpleWorkflow Started. WorkflowID: '{workflowId}'");
});

app.Run();
