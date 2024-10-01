namespace DownloadWatcher.Application;

public class RulesText
{
    public List<string> Text = [];
    public RulesText(string path)
    {
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
        StreamReader sr = new(filename);
        List<string> lines = [];
        try
        {
            string? line = sr.ReadLine();
            while (line != null)
            {
                lines.Add(line);
                line = sr.ReadLine();
            }
        }
        finally
        {
            sr.Close();
        }
        return lines;
    }

}
