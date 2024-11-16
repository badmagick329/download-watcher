using System.Diagnostics;
using DownloadWatcher.Application;
using DownloadWatcher.Core;

namespace DownloadWatcher.Console;

class Program
{
    public static async Task<int> Main(string[] args)
    {
        return await Run(args);
    }

    private static async Task<int> Run(string[] args)
    {
        CliConfig cliConfig = await CliConfig.FromArgs(args);
        if (cliConfig.ValidationErrors.Count != 0)
        {
            foreach (string error in cliConfig.ValidationErrors)
            {
                System.Console.WriteLine(error);
            }

            return -1;
        }

        Debug.Assert(cliConfig.DownloadDirectory != null, "cliConfig.DownloadDirectory != null");
        Debug.Assert(cliConfig.RulesFile != null, "cliConfig.RulesFile != null");

        if (cliConfig.MoveNow)
        {
            Log.WriteLine($"Checking {cliConfig.DownloadDirectory.Name} and moving files");
            RulesText rulesText = new(cliConfig.RulesFile.Name);
            Application.Application app = new(rulesText.Text);
            foreach (string file in Directory.GetFiles(cliConfig.DownloadDirectory.Name))
            {
                app.ProcessChange(file, instant: true);
            }

            return 0;
        }

        StartWatcherAndBlock(cliConfig.DownloadDirectory.Name, cliConfig.RulesFile.Name);
        return 0;
    }

    private static void StartWatcherAndBlock(string downloadDir, string rulesFile)
    {
        FileSystemWatcher watcher =
            new()
            {
                Path = downloadDir,
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite,
                Filter = "*.*",
            };
        RulesText rulesText = new(rulesFile);
        Application.Application app = new(rulesText.Text);
        watcher.Created += (source, e) => OnCreate(source, e, app);
        watcher.Renamed += (source, e) => OnChange(source, e, app);
        watcher.EnableRaisingEvents = true;
        System.Console.WriteLine("Press q to quit");
        Log.WriteLine($"Watching {downloadDir}");
        while (System.Console.Read() != 'q')
            ;
    }

    static void OnCreate(object source, FileSystemEventArgs e, Application.Application app)
    {
        Log.WriteLine($"File: {e.FullPath} {e.ChangeType}");
        app.ProcessChange(e.FullPath);
    }

    static void OnChange(object source, FileSystemEventArgs e, Application.Application app)
    {
        Log.WriteLine($"File: {e.FullPath} {e.ChangeType}");
        app.ProcessChange(e.FullPath);
    }
}