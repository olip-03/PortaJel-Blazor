namespace PortaJel_Blazor.Classes;

public class SyncStatusInfo
{
    public TaskStatus TaskStatus { get; set; } = TaskStatus.WaitingToRun;
    public int StatusPercentage { get; set; } = 0;
}