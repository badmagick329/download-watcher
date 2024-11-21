using System.CommandLine;
using System.Diagnostics;

namespace DownloadWatcher.Core;

public class CliConfig
{
    public required DownloadDirectory DownloadDirectory { get; init; }
    public required RulesFile RulesFile { get; init; }
    public bool MoveNow { get; private init; }
    public int MoveDelay { get; private init; }

    private CliConfig() { }

    public static async Task<Result<CliConfig>> CreateFromArgs(string[] args)
    {
        var directoryOption = new Option<DirectoryInfo?>(
            ["--directory", "-d"],
            "The path to the downloads directory"
        )
        {
            IsRequired = true,
        };
        var rulesFileOption = new Option<FileInfo?>(
            ["--rules-file", "-r"],
            () => new FileInfo("rules.txt"),
            "The path to the file containing the rules for file organization. Defaults to rules.txt in the current directory"
        );
        var moveNowOption = new Option<bool>(
            ["--move-now", "-m"],
            "Run once to scan downloads directory and move files. Skip watch mode. Defaults to false"
        );

        var moveDelayOption = new Option<int>(
            ["--move-delay", "-md"],
            () => 0,
            "The delay in seconds before moving a file after it has been downloaded. Defaults to 0"
        );

        var rootCommand = new RootCommand(
            "Monitor a specified download directory for changes and move files based on predefined rules in a text file"
        )
        {
            directoryOption,
            rulesFileOption,
            moveNowOption,
            moveDelayOption,
        };

        DirectoryInfo? downloadDirectoryInfo = null;
        FileInfo? rulesFileInfo = null;
        bool moveNowValue = false;
        int moveDelayValue = 0;

        rootCommand.SetHandler(
            (directory, rulesFile, moveNow, moveDelay) =>
            {
                downloadDirectoryInfo = directory;
                rulesFileInfo = rulesFile;
                moveNowValue = moveNow;
                moveDelayValue = moveDelay;
            },
            directoryOption,
            rulesFileOption,
            moveNowOption,
            moveDelayOption
        );
        await rootCommand.InvokeAsync(args);

        return AsResult(downloadDirectoryInfo, rulesFileInfo, moveNowValue, moveDelayValue);
    }

    private static Result<CliConfig> AsResult(
        DirectoryInfo? downloadDirectoryInfo,
        FileInfo? rulesFileInfo,
        bool moveNowValue,
        int moveDelayValue
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
            };
        return Result<CliConfig>.Success(cliConfig);
    }

    public override string ToString()
    {
        return $"Download Directory: {DownloadDirectory?.Info?.FullName}\n"
            + $"Rules File: {RulesFile?.Info?.FullName}\n"
            + $"Move Now: {MoveNow}";
    }
}
