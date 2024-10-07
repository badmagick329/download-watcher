public static class Log
{
    private static string NowString()
    {
        return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }

    public static void WriteLine(string message)
    {
        Console.WriteLine($"[{NowString()}] {message}");
    }
}