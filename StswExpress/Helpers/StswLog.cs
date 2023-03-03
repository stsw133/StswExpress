using System;
using System.IO;
using System.Reflection;

namespace StswExpress;

public static class StswLog
{
    public static readonly string LogFileDirectory = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty, "logs");

    /// Write text to log file.
    public static void Write(string text)
    {
        if (!Directory.Exists(LogFileDirectory))
            Directory.CreateDirectory(LogFileDirectory);

        using var sw = new StreamWriter(Path.Combine(LogFileDirectory, $"log_{DateTime.Now:yyyy-MM-dd}.log"), true);
        sw.WriteLine($"{DateTime.Now:yyyy-MM-dd  HH:mm:ss.fff} | {text}");
    }
}
