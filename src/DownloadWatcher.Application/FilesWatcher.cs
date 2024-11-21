using System.Diagnostics;
using DownloadWatcher.Application;
using DownloadWatcher.Core;

class FilesWatcher
{
    private readonly FileSystemWatcher _watcher;
    private readonly CancellationTokenSource _cts;
    private Func<Task>? ScheduledTaskHandler { get; set; }
    private Worker? Worker { get; set; }
    private const int _retryDelay = 2;

    public FilesWatcher(string downloadDirectoryName, CancellationTokenSource cts)
    {
        _watcher = new()
        {
            Path = downloadDirectoryName,
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite,
            Filter = "*.*",
            EnableRaisingEvents = true,
        };
        _cts = cts;
    }

    public void AddScheduledTaskHandler(Func<Task> handler)
    {
        ScheduledTaskHandler = handler;
    }

    public void MonitorFileChanges(int moveDelay)
    {
        Debug.Assert(ScheduledTaskHandler != null, "ScheduledTaskHandler != null");

        Worker = new Worker(_cts.Token);
        Worker.Start();

        void eventHandler(object source, FileSystemEventArgs e)
        {
            ScheduledTask scheduledTask =
                new(
                    async () => await ScheduledTaskHandler!(),
                    DateTime.Now.AddSeconds(moveDelay),
                    0,
                    TimeSpan.FromSeconds(_retryDelay)
                );
            Worker.AddTask(scheduledTask);
            Log.WriteLine($"{e.FullPath} - {e.ChangeType}. Processing in {moveDelay} seconds");
        }

        _watcher.Created += eventHandler;
        _watcher.Renamed += eventHandler;
    }

    public async Task Shutdown()
    {
        Debug.Assert(Worker != null, "Worker != null");
        await _cts.CancelAsync();
        await Worker.Shutdown();
    }
}
