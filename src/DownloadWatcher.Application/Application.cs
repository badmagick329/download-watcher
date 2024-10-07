
namespace DownloadWatcher.Application;

public class Application
{
    public FileRelocation Relocation { get; }
    public Application(List<string> rulesText)
    {
        Relocation = new(rulesText);
    }

    public void ProcessChange(string path, bool instant = false)
    {
        string? newName = Relocation.NewName(path);
        if (newName == null)
        {
            return;
        }

        MonitoredFile monitoredFile = new(path);
        if (!instant && monitoredFile.IsFileStillDownloading())
        {
            Log.WriteLine($"File is still downloading: {path}");
            ProcessChange(path, instant);
            return;
        }

        string message = monitoredFile.TryMoveTo(newName);
        if (message != "")
        {
            Log.WriteLine(message);
        }
    }
}
