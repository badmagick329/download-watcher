namespace DownloadWatcher.Application;

public class Application
{
    public FileRelocation Relocation { get; }
    public Application(List<string> rulesText)
    {
        Console.WriteLine($"RulesText: {string.Join(Environment.NewLine, rulesText)}");
        Relocation = new(rulesText);

        Console.WriteLine($"Rules: {string.Join(Environment.NewLine, Relocation.FileRules)}");
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
            DirectoryInfo? parent = Directory.GetParent(newName);
            if (parent != null && !Path.Exists(parent.FullName))
            {
                Directory.CreateDirectory(parent.FullName);
            }
            Console.WriteLine($"Moving file to {newName}");
            File.Move(path, newName);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error moving file: {e.Message}");
        }
    }
}
