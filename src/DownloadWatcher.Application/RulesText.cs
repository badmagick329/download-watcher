namespace DownloadWatcher.Application;

public class RulesText
{
    public List<string> Text = [];
    public RulesText(string path)
    {
        if (path == string.Empty)
        {
            path = "rules.txt";
        }
        if (!Path.Exists(path))
        {
            throw new FileNotFoundException($"Rules file not found: {path}");
        }
        try
        {
            Text = ReadFile(path);
        }
        catch (Exception e)
        {
            throw new Exception($"Error reading rules file: {e.Message}");
        }
    }
    public static List<string> ReadFile(string filename)
    {
        List<string> lines = [];
        using (StreamReader sr = new(filename))
        {
            string? line;
            while ((line = sr.ReadLine()) != null)
            {
                lines.Add(line);
            }
        }
        return lines;
    }

}
