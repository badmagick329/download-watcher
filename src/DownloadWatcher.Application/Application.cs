
namespace DownloadWatcher.Application;

public class Application
{
    public FileRelocation Relocation { get; }
    public Application(List<string> rulesText)
    {
        Relocation = new(rulesText);
    }

    public void Process(string path, bool instant = false)
    {
        string? newName = Relocation.NewName(path);
        if (newName == null)
        {
            return;
        }
        try
        {
            FileInfo fileInfo = new(path);
            if (!instant && IsFileStillDownloading(fileInfo))
            {
                return;
            }

            DirectoryInfo? parent = Directory.GetParent(newName);
            newName = GetAvailableName(newName);

            if (parent != null && !Path.Exists(parent.FullName))
            {
                Directory.CreateDirectory(parent.FullName);
            }
            Console.WriteLine($"[{NowString()}] Moving file to {newName}");
            File.Move(path, newName);
        }
        catch (IOException e)
        {
            Console.WriteLine($"[{NowString()}] IO Error while moving file: {e.Message}");
        }
        catch (Exception e)
        {
            Console.WriteLine($"[{NowString()}] Error moving file: {e.Message}");
        }
    }

    private static string GetAvailableName(string newName)
    {
        string? directory = Path.GetDirectoryName(newName);
        if (directory == null)
        {
            return newName;
        }

        string fileName = Path.GetFileNameWithoutExtension(newName);
        string extension = Path.GetExtension(newName);
        int count = 1;
        while (Path.Exists(newName))
        {
            newName = Path.Combine(directory, $"{fileName} ({count}){extension}");
            count++;
        }
        return newName;
    }

    static string NowString()
    {
        return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }
    static bool IsFileStillDownloading(FileInfo fileInfo)
    {
        long initialSize = fileInfo.Length;
        Thread.Sleep(1500);
        fileInfo.Refresh();
        return fileInfo.Length != initialSize;
    }
}
