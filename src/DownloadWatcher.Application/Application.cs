namespace DownloadWatcher.Application;

public class Application
{
    public FileRelocation Relocation { get; }
    public Application()
    {
        Relocation = new([@"@""^.*\.(mp4|avi|webm|mkv|ts)$"" E:\temp"]);
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
            Console.WriteLine($"Moving file to {newName}");
            File.Move(path, newName);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error moving file: {e.Message} {e}");
        }
    }
}
