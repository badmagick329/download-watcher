namespace DownloadWatcher.Core;

public class ScheduledTask
{
    public Func<Task> TaskAction { get; set; }
    public DateTime ScheduledTime { get; set; }
    public int RetryCount { get; set; }
    public TimeSpan RetryInterval { get; set; }

    public ScheduledTask(
        Func<Task> taskAction,
        DateTime scheduledTime,
        int retryCount,
        TimeSpan retryInterval
    )
    {
        TaskAction = taskAction;
        ScheduledTime = scheduledTime;
        RetryCount = retryCount;
        RetryInterval = retryInterval;
    }
}
