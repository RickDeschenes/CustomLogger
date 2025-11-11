namespace CustomLogger;

public static class LoggerFactory
{
    public static ICustomLogger Create(string filePath, string fileName, LogLevel minLevel = LogLevel.Debug)
    {
        if (ValidateFile(filePath) == false)
        {
            throw new ArgumentException("Invalid file path provided for logger.");
        }
        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new ArgumentException("File name must not be null or empty.", nameof(fileName));
        }

        var filename = GenerateFileName(filePath, fileName);
        return new CustomLogger(filename, minLevel);
    }

    private static string GenerateFileName(string filePath, string fileName)
    {
        var fullPath = Path.Combine(filePath, fileName);

        //create a new file or use existing if less than 1024 mb
        FileInfo fi = new(fullPath);
        if (fi.Exists && fi.Length > 1024 * 1024)
        {
            // reename existing file with timestamp
            var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            var newFileName = $"{Path.GetFileNameWithoutExtension(fileName)}_{timestamp}{Path.GetExtension(fileName)}";
            var newFullPath = Path.Combine(filePath, newFileName);
            fi.MoveTo(newFullPath);
        }
        // return full path (includes file name)
        return fullPath;

    }

    private static bool ValidateFile(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return false;
        }

        try
        {
            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }

        }
        catch (Exception ex)
        {
            throw new ArgumentException("Error validating file path.", ex);
        }
        return true;
    }
}