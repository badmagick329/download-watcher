using System.CommandLine;
using System.Diagnostics;

namespace DownloadWatcher.Core;

public class CliConfig
{
    public required DownloadDirectory DownloadDirectory { get; init; }
    public required RulesFile RulesFile { get; init; }
    public bool MoveNow { get; private init; }
    public int MoveDelay { get; private init; }
    public bool EnableDebug { get; private init; }

    private CliConfig() { }

    public static async Task<Result<CliConfig>> CreateFromArgs(string[] args)
    {
        var directoryOption = new Option<DirectoryInfo?>(
            ["--directory", "-d"],
            "Directory to monitor for downloads."
        )
        {
            IsRequired = true,
        };
        var rulesFileOption = new Option<FileInfo?>(
            ["--rules-file", "-r"],
            () => new FileInfo("rules.txt"),
            "File containing rules for moving downloads."
        );
        var moveNowOption = new Option<bool>(
            ["--move-now", "-m"],
            "Move existing files immediately."
        );

        var moveDelayOption = new Option<int>(
            ["--move-delay", "-md"],
            () => 0,
            "Delay in seconds before moving files."
        );

        var debugOption = new Option<bool>("--debug", () => false, "Enable debug logging.");

        var rootCommand = new RootCommand(HelpDescription())
        {
            directoryOption,
            rulesFileOption,
            moveNowOption,
            moveDelayOption,
            debugOption,
        };

        DirectoryInfo? downloadDirectoryInfo = null;
        FileInfo? rulesFileInfo = null;
        bool moveNowValue = false;
        int moveDelayValue = 0;
        bool enableDebugValue = false;

        rootCommand.SetHandler(
            (directory, rulesFile, moveNow, moveDelay, enableDebug) =>
            {
                downloadDirectoryInfo = directory;
                rulesFileInfo = rulesFile;
                moveNowValue = moveNow;
                moveDelayValue = moveDelay;
                enableDebugValue = enableDebug;
            },
            directoryOption,
            rulesFileOption,
            moveNowOption,
            moveDelayOption,
            debugOption
        );
        await rootCommand.InvokeAsync(args);

        return AsResult(
            downloadDirectoryInfo,
            rulesFileInfo,
            moveNowValue,
            moveDelayValue,
            enableDebugValue
        );
    }

    private static Result<CliConfig> AsResult(
        DirectoryInfo? downloadDirectoryInfo,
        FileInfo? rulesFileInfo,
        bool moveNowValue,
        int moveDelayValue,
        bool enableDebug
    )
    {
        var downloadDirectoryResult = DownloadDirectory.Create(downloadDirectoryInfo);
        var rulesFileResult = RulesFile.Create(rulesFileInfo);

        if (!(downloadDirectoryResult.IsSuccess && rulesFileResult.IsSuccess))
        {
            return Result<CliConfig>.Failure(
                downloadDirectoryResult.Errors.Concat(rulesFileResult.Errors)
            );
        }
        Debug.Assert(downloadDirectoryResult.Value != null);
        Debug.Assert(rulesFileResult.Value != null);

        CliConfig cliConfig =
            new()
            {
                DownloadDirectory = downloadDirectoryResult.Value,
                RulesFile = rulesFileResult.Value,
                MoveNow = moveNowValue,
                MoveDelay = moveDelayValue,
                EnableDebug = enableDebug,
            };
        return Result<CliConfig>.Success(cliConfig);
    }

    public override string ToString()
    {
        return $"Download Directory: {DownloadDirectory?.Info?.FullName}\n"
            + $"Rules File: {RulesFile?.Info?.FullName}\n"
            + $"Move Now: {MoveNow}\n"
            + $"Move Delay: {MoveDelay}\n"
            + $"Enable Debug: {EnableDebug}";
    }

    private static string HelpDescription()
    {
        var sep = '\\';
        var downloadDir = "C:\\Users\\username\\Downloads";
        if (!OperatingSystem.IsWindows())
        {
            sep = '/';
            downloadDir = "/home/username/Downloads";
        }
        return $@"Watch a directory for downloads and move files based on rules.

Monitor downloads directory with default rules file:
.{sep}watcher -d {downloadDir}

Move existing files immediately:
.{sep}watcher -d {downloadDir} -r .{sep}rules.txt --move-now

Add 10 minute delay before moving files:
.{sep}watcher -d {downloadDir} --move-delay 600

Enable debug logging:
.{sep}watcher -d {downloadDir} --debug";
    }
}
