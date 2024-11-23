namespace DownloadWatcher.Core;

public static class Ansii
{
    private const string Reset = "\u001b[0m";
    private const string Black = "\u001b[30m";
    private const string Red = "\u001b[31m";
    private const string Green = "\u001b[32m";
    private const string Yellow = "\u001b[33m";
    private const string Blue = "\u001b[34m";
    private const string Magenta = "\u001b[35m";
    private const string Cyan = "\u001b[36m";
    private const string White = "\u001b[37m";

    public static string BlackText<T>(T value) => $"{Black}{value}{Reset}";

    public static string RedText<T>(T value) => $"{Red}{value}{Reset}";

    public static string GreenText<T>(T value) => $"{Green}{value}{Reset}";

    public static string YellowText<T>(T value) => $"{Yellow}{value}{Reset}";

    public static string BlueText<T>(T value) => $"{Blue}{value}{Reset}";

    public static string MagentaText<T>(T value) => $"{Magenta}{value}{Reset}";

    public static string CyanText<T>(T value) => $"{Cyan}{value}{Reset}";

    public static string WhiteText<T>(T value) => $"{White}{value}{Reset}";
}
