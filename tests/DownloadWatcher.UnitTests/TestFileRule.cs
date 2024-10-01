namespace DownloadWatcher.UnitTests;

public class TestFileRule
{
    [Theory]
    [InlineData(@"@""^.*\.(mp4|avi)$"" C:\My Videos", "^.*\\.(mp4|avi)$", @"C:\My Videos")]
    [InlineData(@"@""^.*\.(jpg|jpeg|png)$"" C:\My Pictures", "^.*\\.(jpg|jpeg|png)$", @"C:\My Pictures")]
    [InlineData(@"@""^.*\d{4}\.txt$"" C:\My Documents", "^.*\\d{4}\\.txt$", @"C:\My Documents")]
    [InlineData(@"@""^Report_\d{2}-\d{2}-\d{4}\.pdf$"" D:\Reports", "^Report_\\d{2}-\\d{2}-\\d{4}\\.pdf$", @"D:\Reports")]
    [InlineData(@"@""^.*\.(docx|xlsx|pptx)$"" E:\OfficeFiles", "^.*\\.(docx|xlsx|pptx)$", @"E:\OfficeFiles")]
    public void FileRule_ValidRuleText_SetsPropertiesCorrectly(string text, string regex, string path)
    {
        FileRule fileRule = new(text);
        Assert.Equal(regex, fileRule.RegexPattern);
        Assert.Equal(path, fileRule.TargetPath);
    }

}