public class MonitoredFile
{
    public string FilePath { get; }
    public FileInfo FileInfo { get; }
    public MonitoredFile(string path)
    {
        FilePath = path;
        FileInfo = new(path);
    }

    /// <summary>
    /// Attempts to move the file to a new location with a new name.
    /// </summary>
    /// <param name="newName">The new name and path for the file.</param>
    /// <returns>
    /// An empty string if the move is successful, or an error message if the move fails.
    /// </returns>
    /// <remarks>
    /// If the file is empty, the move is ignored and a message is returned.
    /// If the target directory does not exist, it is created.
    /// If a file with the new name already exists, a unique name is generated.
    /// The method returns an error message if the move fails.
    /// </remarks>
    public string TryMoveTo(string newName)
    {
        if (FileInfo.Length == 0)
        {
            return $"Ignoring empty file {FilePath}";
        }
        DirectoryInfo? parent = Directory.GetParent(newName);
        newName = GetAvailableName(newName);
        if (parent != null && !Path.Exists(parent.FullName))
        {
            Directory.CreateDirectory(parent.FullName);
        }
        Log.WriteLine($"Moving file to {newName}");

        try
        {
            File.Move(FilePath, newName);
            return "";
        }
        catch (IOException e)
        {
            return $"IO Error while moving file: {e.Message}";
        }
        catch (Exception e)
        {
            return $"Error moving file: {e.Message}";
        }
    }


    public bool IsFileStillDownloading()
    {
        long initialSize = FileInfo.Length;
        Thread.Sleep(1500);
        FileInfo.Refresh();
        return FileInfo.Length != initialSize;
    }

    private static string GetAvailableName(string newName)
    {
        if (!Path.Exists(newName))
        {
            return newName;
        }

        DirectoryInfo? directory = Directory.GetParent(newName);
        if (directory == null)
        {
            return newName;
        }

        string fileName = Path.GetFileNameWithoutExtension(newName);
        string extension = Path.GetExtension(newName);
        int count = 1;
        while (Path.Exists(newName))
        {
            newName = Path.Combine(directory.FullName, $"{fileName} ({count}){extension}");
            count++;
        }
        return newName;
    }

}