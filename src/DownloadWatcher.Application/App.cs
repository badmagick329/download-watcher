namespace DownloadWatcher.Application;

public class App
{
    private FileRelocation Relocation { get; init; }
    private string DownloadDirectoryName { get; init; }
    private readonly int _moveDelay;

    public App(string downloadDirectoryName, string rulesFileName, int moveDelay = 0)
    {
        DownloadDirectoryName = downloadDirectoryName;
        Relocation = new FileRelocation(new RulesText(rulesFileName).Lines);
        _moveDelay = moveDelay;
    }

    public async Task<int> RunAsync()
    {
        using var cts = new CancellationTokenSource();
        FilesWatcher watcher = new(DownloadDirectoryName, cts);
        watcher.AddScheduledTaskHandler(
            async (string path) => await ProcessFileChangeEventAsync(path)
        );
        watcher.MonitorFileChanges(_moveDelay);

        using var sr = new StreamReader(Console.OpenStandardInput());
        Console.WriteLine("Type q to quit");
        string? line = string.Empty;
        while (line != null && !line.Trim().Equals("q", StringComparison.CurrentCultureIgnoreCase))
        {
            line = await sr.ReadLineAsync();
        }

        Console.WriteLine("Shutting down...");
        await watcher.Shutdown();
        return 0;
    }

    public async Task<int> RunOnce()
    {
        Log.WriteLine($"Checking {DownloadDirectoryName} and moving files");
        foreach (string file in Directory.GetFiles(DownloadDirectoryName))
        {
            await ProcessFileChangeEventAsync(file, instant: true);
        }
        return 0;
    }

    public async Task ProcessFileChangeEventAsync(string path, bool instant = false)
    {
        string? newName = Relocation.NewName(path);
        if (newName == null)
        {
            return;
        }

        MonitoredFile monitoredFile = new(path);
        if (!instant && (await monitoredFile.IsFileStillDownloadingAsync()))
        {
            Log.WriteLine($"File is still downloading: {path}");
            await ProcessFileChangeEventAsync(path, instant);
            return;
        }

        string message = monitoredFile.TryMoveTo(newName);
        if (message != "")
        {
            Log.WriteLine(message);
        }
    }
}
