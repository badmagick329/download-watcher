using DownloadWatcher.Application;

class Program
{
    public static void Main(string[] args)
    {
        string[] fullArgs = Environment.GetCommandLineArgs();
        if (fullArgs.Length < 2)
        {
            string helpMessage = @$"Usage: {fullArgs[0]} <Download Folder> [Rules File]

Arguments:
  <Download Folder>  The path to the folder where downloads are stored. This argument is required.
  [Rules File]       (Optional) The path to the file containing the rules for file organization.
                     If not provided, defaults to looking for rules.txt in the current path";

            Console.WriteLine(helpMessage);
            return;
        }
        string rulesFile = string.Empty;
        if (fullArgs.Length > 2)
        {
            rulesFile = fullArgs[2];
        }
        Run(fullArgs[1], rulesFile);
    }


    public static void Run(string downloadDir, string rulesFile)
    {
        FileSystemWatcher watcher = new()
        {
            Path = downloadDir,
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite,
            Filter = "*.*",
        };
        RulesText rulesText = new(rulesFile);
        Console.WriteLine(string.Join(Environment.NewLine, rulesText.Text));
        Application app = new(rulesText.Text);
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