using System.Text.RegularExpressions;

public partial class FileRule
{
    public string RegexPattern { get; set; } = string.Empty;
    public string TargetPath { get; set; } = string.Empty;
    public bool IsEmpty
    {
        get => TargetPath == string.Empty || RegexPattern == string.Empty;
    }

    public FileRule(string ruleText)
    {
        Match match = RegexAndPathPattern().Match(ruleText);
        if (match.Success)
        {
            RegexPattern = match.Groups[1].Value;
            TargetPath = match.Groups[2].Value;
            ParseTilda();
        }
    }

    [GeneratedRegex(@"^@""([^""]+)""\s+(.+)")]
    private static partial Regex RegexAndPathPattern();

    public override string ToString() => $"{RegexPattern} {TargetPath}";

    private void ParseTilda()
    {
        if (!TargetPath.StartsWith("~"))
        {
            return;
        }

        char splitChar = TargetPath.Contains('/') ? '/' : '\\';
        string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        if (TargetPath[1] != splitChar)
        {
            home = $"{home}{splitChar}";
        }
        TargetPath = TargetPath.Replace("~", home);
    }
}
