using Mqtt.AspNetCore;
using MQTTnet;
using System.Text.Json;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace WorkflowCore.Monitor.Consumers;

public class WorkflowInstanceConsumer(
    IWorkflowHost host) : IMqttConsumer
{
    private readonly IWorkflowHost _host = host;

    public async Task HandleAsync(MqttApplicationMessageReceivedEventArgs message)
    {
        var json = message.ApplicationMessage.ConvertPayloadToString();

        var workflowInstance = JsonSerializer.Deserialize<WorkflowInstance>(json, JsonSerializerOptions.Web)!;

        var exists = await WorkflowInstanceExists(workflowInstance.Id);
        if (!exists)
        {
            await _host.PersistenceStore.CreateNewWorkflow(workflowInstance);
        }
    }

    private async Task<bool> WorkflowInstanceExists(string id)
    {
        try
        {
            var existing = await _host.PersistenceStore.GetWorkflowInstance(id);
            return existing != null;
        }
        catch
        {
            return false;
        }
    }
}
