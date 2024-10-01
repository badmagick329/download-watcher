using DownloadWatcher.Application;

class Program
{
    public static void Main(string[] args)
    {
        string[] fullArgs = Environment.GetCommandLineArgs();
        if (fullArgs.Length < 2)
        {
            Console.WriteLine($"Usage: {fullArgs[0]} <Download Directory>");
            return;
        }
        Run(fullArgs[1]);
    }


    public static void Run(string downloadDir)
    {
        FileSystemWatcher watcher = new()
        {
            Path = downloadDir,
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite,
            Filter = "*.*",
        };
        Application app = new();
        watcher.Created += (source, e) => OnCreate(source, e, app);
        watcher.Renamed += (source, e) => OnChange(source, e, app);
        watcher.EnableRaisingEvents = true;
        Console.WriteLine("Press q to quit");
        Console.WriteLine($"Watching {downloadDir}");
        while (Console.Read() != 'q') ;

    }
    static void OnCreate(object source, FileSystemEventArgs e, Application app)
    {
        Console.WriteLine($"File: {e.FullPath} {e.ChangeType}");
        app.Process(e.FullPath);
    }
    static void OnChange(object source, FileSystemEventArgs e, Application app)
    {
        Console.WriteLine($"File: {e.FullPath} {e.ChangeType}");
        app.Process(e.FullPath);
    }
}