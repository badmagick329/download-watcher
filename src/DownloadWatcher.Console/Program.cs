using System.Diagnostics;
using DownloadWatcher.Application;
using DownloadWatcher.Core;

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

        if (cliConfig.MoveNow)
        {
            return await app.RunOnce();
        }

        return await app.RunAsync();
    }
}
