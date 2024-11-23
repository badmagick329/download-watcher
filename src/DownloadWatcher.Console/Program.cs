using System.Diagnostics;
using DownloadWatcher.Application;
using DownloadWatcher.Core;
using Serilog;
using Serilog.Events;

namespace DownloadWatcher.Console;

class Program
{
    public static async Task<int> Main(string[] args)
    {
        return await RunAsync(args);
    }

    private static async Task<int> RunAsync(string[] args)
    {
        var configResult = await CliConfig.CreateFromArgs(args);
        if (!configResult.IsSuccess)
        {
            foreach (string error in configResult.Errors)
            {
                System.Console.WriteLine(error);
            }

            return -1;
        }

        CliConfig cliConfig = configResult.Value;
        Debug.Assert(configResult.Value != null);
        App app =
            new(cliConfig.DownloadDirectory.Name, cliConfig.RulesFile.Name, cliConfig.MoveDelay);

        CreateLogger(cliConfig.EnableDebug);
        Log.Debug("Arguments: {args}", args);
        if (cliConfig.MoveNow)
        {
            return await app.RunOnce();
        }

        Log.Information($"Monitoring {cliConfig.DownloadDirectory.Name} for file changes.");
        if (cliConfig.MoveDelay > 0)
        {
            Log.Information(
                $"Files will be moved after {cliConfig.MoveDelay} seconds of creation or rename."
            );
        }
        return await app.RunAsync();
    }

    private static void CreateLogger(bool enableDebug)
    {
        var logger = new LoggerConfiguration();
        if (enableDebug)
        {
            logger = logger
                .MinimumLevel.Debug()
                .WriteTo.File(
                    "log-.txt",
                    restrictedToMinimumLevel: LogEventLevel.Debug,
                    rollingInterval: RollingInterval.Day
                );
        }
        logger = logger.WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Information);
        Log.Logger = logger.CreateLogger();
    }
}
