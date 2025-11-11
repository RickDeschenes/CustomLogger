using System;
using System.IO;

namespace CustomLogger;

/// <summary>
/// Logger that returns the log entry to the method
/// </summary>
public interface ICustomLogger
{
    /// <summary>
    /// Returns the log message
    /// </summary>
    string Debug(string message, string? className = null, string? methodName = null);

    /// <summary>
    /// Returns the log message
    /// </summary>
    string Info(string message, string? className = null, string? methodName = null);

    /// <summary>
    /// Returns the log message
    /// </summary>
    string Warning(string message, string? className = null, string? methodName = null);

    // Keep Exception parameter for Error, append optional class/method context parameters.
    /// <summary>
    /// Returns the log message
    /// </summary>
    string Error(string message, Exception ex, string? className = null, string? methodName = null);
}

public class CustomLogger(string filePath, LogLevel minLevel) : ICustomLogger
{
    private readonly string _filePath = filePath;
    private readonly LogLevel _minLevel = minLevel;

    public string Debug(string message, string? className = null, string? methodName = null)
    {
        var results = ComposeLog("DEBUG", message, className, methodName);
        WriteData(results, LogLevel.Debug);
        return results;
    }

    public string Info(string message, string? className = null, string? methodName = null)
    {
        var results = ComposeLog("INFO", message, className, methodName);
        WriteData(results, LogLevel.Info);
        return results;
    }

    public string Warning(string message, string? className = null, string? methodName = null)
    {
        var results = ComposeLog("WARNING", message, className, methodName);
        WriteData(results, LogLevel.Warning);
        return results;
    }

    public string Error(string message, Exception ex, string? className = null, string? methodName = null)
    {
        var results = ComposeLog("ERROR", message, className, methodName, ex);
        WriteData(results, LogLevel.Error);
        return results;
    }

    private static string ComposeLog(string levelText, string message, string? className, string? methodName, Exception? ex = null)
    {
        var contextParts = "";

        if (!string.IsNullOrWhiteSpace(className))
            contextParts += $"[Class: {className}] ";

        if (!string.IsNullOrWhiteSpace(methodName))
            contextParts += $"[Method: {methodName}] ";

        var composed = $"{levelText}: {contextParts}{message}";

        if (ex is not null)
            composed += $" - Exception: {ex.Message}";

        return composed;
    }

    private void WriteData(string logMessage, LogLevel logLevel)
    {
        // Skip logging if below minimum level
        if (logLevel < _minLevel)
            return;

        using var writer = new StreamWriter(_filePath, append: true);
        writer.WriteLine(logMessage);
    }
}
