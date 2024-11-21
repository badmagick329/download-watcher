namespace DownloadWatcher.Core;

public abstract class FileSystemEntity<T>
    where T : FileSystemInfo
{
    public T Info { get; init; }
    public string Name => Info.FullName;

    protected FileSystemEntity(T info) => Info = info;

    protected static Result<TResult> Create<TResult>(
        T? info,
        string entityType,
        Func<T, TResult> factory
    )
        where TResult : FileSystemEntity<T>
    {
        if (info == null)
        {
            return Result<TResult>.Failure([$"{entityType} is required"]);
        }

        if (!info.Exists)
        {
            return Result<TResult>.Failure([$"\"{info.FullName}\" does not exist"]);
        }

        return Result<TResult>.Success(factory(info));
    }
}

public class DownloadDirectory : FileSystemEntity<DirectoryInfo>
{
    public DownloadDirectory(DirectoryInfo info)
        : base(info) { }

    public static Result<DownloadDirectory> Create(DirectoryInfo? info) =>
        Create(info, "Download directory", dir => new DownloadDirectory(dir));
}

public class RulesFile : FileSystemEntity<FileInfo>
{
    public RulesFile(FileInfo info)
        : base(info) { }

    public static Result<RulesFile> Create(FileInfo? info) =>
        Create(info, "Rules file", file => new RulesFile(file));
}
