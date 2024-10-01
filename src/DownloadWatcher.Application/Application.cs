
namespace DownloadWatcher.Application;

public class Application
{
    public FileRelocation Relocation { get; }
    public Application(List<string> rulesText)
    {
        Relocation = new(rulesText);
    }

    public void Process(string path)
    {
        string? newName = Relocation.NewName(path);
        if (newName == null)
        {
            return;
        }
        try
        {
            FileInfo fileInfo = new(path);
            if (IsFileStillDownloading(fileInfo))
            {
                return;
            }

            DirectoryInfo? parent = Directory.GetParent(newName);
            if (parent != null && !Path.Exists(parent.FullName))
            {
                Directory.CreateDirectory(parent.FullName);
            }
            Console.WriteLine($"[{NowString()}] Moving file to {newName}");
            File.Move(path, newName);
        }
        catch (IOException)
        {
            File.Delete(path);
        }
        catch (Exception e)
        {
            Console.WriteLine($"[{NowString()}] Error moving file: {e.Message}");
        }
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
