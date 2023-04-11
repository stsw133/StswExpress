using System;
using System.IO;

namespace StswExpress;

/// <summary>
/// Provides a simple way to write log messages to a file.
/// </summary>
public static class StswLog
{
    /// <summary>
    /// Specifies the path to the directory where the log files will be saved.
    /// </summary>
    public static string DirectoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");

    /// <summary>
    /// Writes a log entry to a file in the directory specified by <see cref="DirectoryPath"/>.
    /// Every log entry is written in a new line and starts with current date and time with a new file created each day.
    /// </summary>
    /// <param name="text">Text to log.</param>
    public static void Write(string text)
    {
        if (!Directory.Exists(DirectoryPath))
            Directory.CreateDirectory(DirectoryPath);

        using (var sw = new StreamWriter(Path.Combine(DirectoryPath, $"log_{DateTime.Now:yyyy-MM-dd}.log"), true))
            sw.WriteLine($"{DateTime.Now:yyyy-MM-dd  HH:mm:ss.fff} | {text}");
    }
}
