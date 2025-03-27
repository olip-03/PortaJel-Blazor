using Portajel.Connections.Data;
using System.Threading;

namespace Portajel.Connections.Services;

public class SyncStatusInfo
{
    /// <summary>
    /// The state of this task.
    /// </summary>
    public TaskStatus TaskStatus { get; set; } = TaskStatus.WaitingToRun;
    /// <summary>
    /// The overall percentage of this sync.
    /// </summary>
    public int StatusPercentage { get; set; } = 0;
    /// <summary>
    /// The amount of items for this type that exist on the server.
    /// </summary>
    public int ServerItemTotal { get;set; } = 0;
    /// <summary>
    /// The amount of items for this type that we're counting, or that we've got in memory.
    /// </summary>
    public int ServerItemCount { get; set; } = 0;
    /// <summary>
    /// The total number of this item found in the database.
    /// </summary>
    public int DbFoundTotal { get; set; } = 0;
}
