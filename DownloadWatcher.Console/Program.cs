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
        TestRun(fullArgs[1]);
    }


    public static void TestRun(string downloadDir)
    {
        FileSystemWatcher watcher = new()
        {
            Path = downloadDir,
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite,
            Filter = "*.*",
        };
        watcher.Created += OnChange;
        watcher.EnableRaisingEvents = true;
        Console.WriteLine("Press q to quit");
        while (Console.Read() != 'q') ;

    }
    static void OnChange(object source, FileSystemEventArgs e)
    {
        Console.WriteLine($"File: {e.FullPath} {e.ChangeType}");

        //  File.Move(e.FullPath, "C:\temp");

    }
}