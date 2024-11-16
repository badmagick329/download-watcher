using System.CommandLine;

namespace DownloadWatcher.Core;

public class CliConfig
{
    public DownloadDirectory? DownloadDirectory { get; private init; }
    public RulesFile? RulesFile { get; private init; }
    public bool MoveNow { get; private init; }
    public List<string> ValidationErrors { get; private set; } = [];

    private CliConfig()
    {
    }

    public static async Task<CliConfig> FromArgs(string[] args)
    {
        var directoryOption = new Option<DirectoryInfo?>(
            ["--directory", "-d"],
            "The path to the downloads directory"
        ) { IsRequired = true };
        var rulesFileOption = new Option<FileInfo?>(
            ["--rules-file", "-r"],
            "The path to the file containing the rules for file organization. Defaults to rules.txt in the current directory"
        );
        var moveNowOption = new Option<bool>(
            ["--move-now", "-m"],
            "Run once to scan downloads directory and move files. Skip watch mode. Defaults to false"
        );

        var rootCommand =
            new RootCommand(
                "Monitor a specified download directory for changes and move files based on predefined rules in a text file")
            {
                directoryOption,
                rulesFileOption,
                moveNowOption
            };

        DirectoryInfo? downloadDirectoryInfo = null;
        FileInfo? rulesFileInfo = null;
        bool moveNowValue = false;

        rootCommand.SetHandler(
            (directory, rulesFile, moveNow) =>
            {
                rulesFile ??= new FileInfo("rules.txt");

                downloadDirectoryInfo = directory;
                rulesFileInfo = rulesFile;
                moveNowValue = moveNow;
            }, directoryOption, rulesFileOption, moveNowOption);

        await rootCommand.InvokeAsync(args);
        CliConfig cliConfig = new CliConfig
        {
            DownloadDirectory = new DownloadDirectory(downloadDirectoryInfo),
            RulesFile = new RulesFile(rulesFileInfo),
            MoveNow = moveNowValue
        };
        cliConfig.Validate();
        return cliConfig;
    }

    private void Validate()
    {
        if (DownloadDirectory is not null && DownloadDirectory.ValidationError != string.Empty)
        {
            ValidationErrors.Add(DownloadDirectory.ValidationError);
        }

        if (RulesFile is not null && RulesFile.ValidationError != string.Empty)
        {
            ValidationErrors.Add(RulesFile.ValidationError);
        }
    }

    public override string ToString()
    {
        return
            $"Download Directory: {DownloadDirectory?.Info?.FullName}\n" +
            $"Rules File: {RulesFile?.Info?.FullName}\n" +
            $"Move Now: {MoveNow}";
    }
}

public class DownloadDirectory
{
    public DirectoryInfo? Info { get; init; }
    public string ValidationError { get; private set; } = string.Empty;
    public string Name => Info?.FullName ?? string.Empty;

    public DownloadDirectory(DirectoryInfo? info)
    {
        Info = info;
        Validate();
    }

    private void Validate()
    {
        if (Info == null)
        {
            ValidationError = "Download directory is required";
            return;
        }

        if (!Directory.Exists(Info.FullName))
        {
            ValidationError = $"\"{Info.FullName}\" does not exist";
        }
    }
}

public class RulesFile
{
    public FileInfo? Info { get; init; }
    public string ValidationError { get; private set; } = string.Empty;
    public string Name => Info?.FullName ?? string.Empty;

    public RulesFile(FileInfo? info)
    {
        Info = info;
        Validate();
    }

    private void Validate()
    {
        if (Info == null)
        {
            return;
        }

        if (!File.Exists(Info.FullName))
        {
            ValidationError = $"\"{Info.FullName}\" does not exist. Ensure the default or specified path exists";
        }
    }
}