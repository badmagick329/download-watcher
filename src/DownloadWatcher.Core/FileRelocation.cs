using System.Text.RegularExpressions;

public class FileRelocation
{
    public List<FileRule> FileRules { get; set; } = [];

    public FileRelocation(List<string> rulesLines)
    {
        FileRules = rulesLines.Select(line => new FileRule(line)).Where(fr => !fr.IsEmpty).ToList();
    }

    public string? NewName(string path)
    {
        try
        {
            EnsureValidPath(path);
        }
        catch
        {
            return null;
        }
        FileRule? fileRule = MatchingRule(path);
        if (fileRule == null)
        {
            return null;
        }
        return Path.Combine(fileRule.TargetPath, Path.GetFileName(path));
    }

    private static void EnsureValidPath(string path)
    {
        if (!Path.Exists(path))
        {
            throw new FileNotFoundException("The specified path does not exist.");
        }

        FileAttributes attr = File.GetAttributes(path);
        if (attr.HasFlag(FileAttributes.Directory))
        {
            throw new ArgumentException("The specified path is a directory.");
        }
    }

    internal FileRule? MatchingRule(string path)
    {
        string filename = Path.GetFileName(path);
        return FileRules.LastOrDefault(rule => Regex.IsMatch(filename, rule.RegexPattern));
    }
}
