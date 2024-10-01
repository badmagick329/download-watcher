
namespace DownloadWatcher.UnitTests;

public class TestFileRelocation
{
    [Theory]
    [MemberData(nameof(MatchingRule_ValidMatch_Data))]
    public static void MatchingRule_ValidMatch_ReturnsCorrespondingFileRule(string rulesText, string regex, string path, string[] inputs)
    {
        FileRelocation fileRelocation = new([rulesText]);
        foreach (string input in inputs)
        {
            FileRule? matchedRule = fileRelocation.MatchingRule(input);
            Assert.Equal(regex, matchedRule!.RegexPattern);
            Assert.Equal(path, matchedRule!.TargetPath);
        }
    }

    [Theory]
    [MemberData(nameof(MatchingRule_InvalidMatch_Data))]
    public static void MatchingRule_InvalidMatch_ReturnsNull(string rulesText, string[] inputs)
    {
        FileRelocation fileRelocation = new([rulesText]);
        foreach (string input in inputs)
        {
            FileRule? matchedRule = fileRelocation.MatchingRule(input);
            Assert.Null(matchedRule);
        }
    }

    public static IEnumerable<object[]> MatchingRule_ValidMatch_Data()
    {
        yield return new object[] {
            @"@""^.*\.(mp4|avi|webm|mkv|ts)$"" C:\My Videos",
            @"^.*\.(mp4|avi|webm|mkv|ts)$",
            @"C:\My Videos",
            new string[] { "video.mp4", "youtube video.webm", "mp4.avi", ".test.123.mkv", " - .ts" }
        };

        yield return new object[] {
            @"@""^.*\.(jpg|jpeg|png)$"" C:\My Pictures",
            @"^.*\.(jpg|jpeg|png)$",
            @"C:\My Pictures",
            new string[] { "image.jpg", "photo.jpeg", "picture.png" }
        };

        yield return new object[] {
            @"@""^.*\d{4}\.txt$"" C:\My Documents",
            @"^.*\d{4}\.txt$",
            @"C:\My Documents",
            new string[] { "file1234.txt", "document5678.txt" }
        };

        yield return new object[] {
            @"@""^Report_\d{2}-\d{2}-\d{4}\.pdf$"" D:\Reports",
            @"^Report_\d{2}-\d{2}-\d{4}\.pdf$",
            @"D:\Reports",
            new string[] { "Report_12-25-2021.pdf", "Report_01-01-2022.pdf" }
        };

        yield return new object[] {
            @"@""^.*\.(docx|xlsx|pptx)$"" E:\OfficeFiles",
            @"^.*\.(docx|xlsx|pptx)$",
            @"E:\OfficeFiles",
            new string[] { "document.docx", "spreadsheet.xlsx", "presentation.pptx" }
        };
    }

    public static IEnumerable<object[]> MatchingRule_InvalidMatch_Data()
    {
        yield return new object[] {
            @"@""^.*\.(mp4|avi|webm|mkv|ts)$"" C:\My Videos",
            new string[] { "video.mp3", "youtube video.mp4a", "mp4.avi.txt", ".test.123.mkvx", " - .ts1" }
        };

        yield return new object[] {
            @"@""^.*\.(jpg|jpeg|png)$"" C:\My Pictures",
            new string[] { "image.bmp", "photo.jpegg", "picture.pn" }
        };

        yield return new object[] {
            @"@""^.*\d{4}\.txt$"" C:\My Documents",
            new string[] { "file123.txt", "document56a789.txt" }
        };

        yield return new object[] {
            @"@""^Report_\d{2}-\d{2}-\d{4}\.pdf$"" D:\Reports",
            new string[] { "Report_12-25-21.pdf", "Report_01-01-202.pdf" }
        };

        yield return new object[] {
            @"@""^.*\.(docx|xlsx|pptx)$"" E:\OfficeFiles",
            new string[] { "document.doc", "spreadsheet.xls", "presentation.ppt" }
        };
    }
}
