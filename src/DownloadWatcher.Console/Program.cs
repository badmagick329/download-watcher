﻿using DownloadWatcher.Application;

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
                     If not provided, defaults to looking for rules.txt in the current path
  --move-now         Scan the downloads directory and move files. Do not enter watch mode";

            Console.WriteLine(helpMessage);
            return;
        }

        // TODO: Refactor?
        string rulesFile = string.Empty;
        const string moveNow = "--move-now";
        if (fullArgs.Length > 2 && fullArgs[2] != moveNow)
        {
            rulesFile = fullArgs[2];
        }

        if (fullArgs[^1] == moveNow || fullArgs[^2] == moveNow)
        {
            Console.WriteLine($"Checking {fullArgs[1]} and moving files");
            RulesText rulesText = new(rulesFile);
            Application app = new(rulesText.Text);
            foreach (string file in Directory.GetFiles(fullArgs[1]))
            {
                app.ProcessChange(file, instant: true);
            }
        }
        else
        {
            Run(fullArgs[1], rulesFile);

        }

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
        Application app = new(rulesText.Text);
        watcher.Created += (source, e) => OnCreate(source, e, app);
        watcher.Renamed += (source, e) => OnChange(source, e, app);
        watcher.EnableRaisingEvents = true;
        Console.WriteLine("Press q to quit");
        Console.WriteLine($"[{NowString()}] Watching {downloadDir}");
        while (Console.Read() != 'q') ;

    }
    static void OnCreate(object source, FileSystemEventArgs e, Application app)
    {
        Console.WriteLine($"[{NowString()}] File: {e.FullPath} {e.ChangeType}");
        app.ProcessChange(e.FullPath);
    }
    static void OnChange(object source, FileSystemEventArgs e, Application app)
    {
        Console.WriteLine($"[{NowString()}] File: {e.FullPath} {e.ChangeType}");
        app.ProcessChange(e.FullPath);
    }

    static string NowString()
    {
        return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }
}