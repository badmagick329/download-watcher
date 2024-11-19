﻿using System;
using System.Diagnostics;
using DownloadWatcher.Application;
using DownloadWatcher.Core;

namespace DownloadWatcher.Console;

class Program
{
    public static async Task<int> Main(string[] args)
    {
        return await Run(args);
        // return await BgRunTesting(args);
    }

    private static async Task<int> BgRunTesting(string[] args)
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

        System.Console.WriteLine(
            $"Watching {cliConfig.DownloadDirectory.Name} with rules file {cliConfig.RulesFile.Name}"
        );
        FileSystemWatcher watcher =
            new()
            {
                Path = cliConfig.DownloadDirectory.Name,
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite,
                Filter = "*.*",
                EnableRaisingEvents = true,
            };
        RulesText rulesText = new(cliConfig.RulesFile.Name);
        Application.Application app = new(rulesText.Text);

        using var sr = new StreamReader(System.Console.OpenStandardInput());
        using var cts = new CancellationTokenSource();
        var cancelToken = cts.Token;
        var worker = new Worker(cancelToken);
        worker.Start();

        watcher.Created += (source, e) =>
        {
            System.Console.WriteLine($"Created {e.FullPath}");
            var task = async () =>
            {
                app.ProcessChange(e.FullPath);
            };
            ScheduledTask scheduledTask =
                new(task, DateTime.Now.AddSeconds(3), 0, TimeSpan.FromSeconds(2));
            worker.AddTask(scheduledTask);
            System.Console.WriteLine($"Added task for {e.FullPath}");
        };

        watcher.Renamed += (source, e) =>
        {
            System.Console.WriteLine($"Created {e.FullPath}");
            var task = async () =>
            {
                app.ProcessChange(e.FullPath);
            };
            ScheduledTask scheduledTask =
                new(task, DateTime.Now.AddSeconds(3), 0, TimeSpan.FromSeconds(2));
            worker.AddTask(scheduledTask);
            System.Console.WriteLine($"Added task for {e.FullPath}");
        };

        System.Console.WriteLine("Type q to quit");
        string? line = string.Empty;
        while (line != null && !line.Trim().Equals("q", StringComparison.CurrentCultureIgnoreCase))
        {
            System.Console.WriteLine($"main thread: {Environment.CurrentManagedThreadId}");
            line = await sr.ReadLineAsync();
        }

        System.Console.WriteLine("Cancelling Worker...");
        await cts.CancelAsync();
        await worker.Shutdown();

        System.Console.WriteLine("Exiting application.");
        return 0;
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
