
class Application
{
    public FileRelocation Relocation { get; }
    public Application()
    {
        Relocation = new([@"@""^.*\.(mp4|avi|webm|mkv|ts)$"" C:\My Videos"]);
    }

    public void Process(string path)
    {
        string? newLocation = Relocation.NewLocation(path);
        Console.WriteLine($"Moving file to {newLocation}");
    }
}