namespace TabulateAI.Models;

public class ProcessingStep
{
    public string Label { get; set; } = string.Empty;
    public ProcessingStepStatus Status { get; set; }
}

public enum ProcessingStepStatus
{
    Pending,
    InProgress,
    Done
}
