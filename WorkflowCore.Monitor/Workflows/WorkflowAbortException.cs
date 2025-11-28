namespace WorkflowCore.Monitor.Workflows;

public class WorkflowAbortException : Exception
{
    public WorkflowAbortException(string message) : base(message)
    {
    }
}
