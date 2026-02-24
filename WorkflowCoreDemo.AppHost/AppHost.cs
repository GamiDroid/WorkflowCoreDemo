var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.WorkflowCore_Monitor>("workflowcore-monitor");

builder.Build().Run();
