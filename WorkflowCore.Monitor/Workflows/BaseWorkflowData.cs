namespace WorkflowCore.Monitor.Workflows;

public class BaseWorkflowData
{
    public string WorkflowId { get; set; } = Guid.NewGuid().ToString();
    public DateTime StartTime { get; set; } = DateTime.Now;
    public DateTime? EndTime { get; set; }
    public List<StepState> StepsExecuted { get; set; } = [];

    public bool IsFinished => EndTime is not null;
    public bool IsStepExecuted(string key) => StepsExecuted.Any(s => s.Key == key && s.EndTime != null);

    public record StepState(string Key, DateTime StartTime)
    {
        public DateTime? EndTime { get; set; }
    }
}
