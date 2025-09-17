using StswExpress.Commons;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace StswExpress.Tests;
public class StswLogTests
{
    private readonly string _logDir;
    private readonly string _archiveDir;

    public StswLogTests()
    {
        _logDir = Path.Combine(Path.GetTempPath(), "StswLogTestLogs");
        _archiveDir = Path.Combine(Path.GetTempPath(), "StswLogTestArchives");
        Directory.CreateDirectory(_logDir);
        Directory.CreateDirectory(_archiveDir);

        StswLog.Config.LogDirectoryPath = _logDir;
        StswLog.Config.Archive.ArchiveDirectoryPath = _archiveDir;
        StswLog.Config.IsLoggingDisabled = false;
        StswLog.Config.LogTypes_DEBUG = Enum.GetValues(typeof(StswInfoType)).Cast<StswInfoType>();
        StswLog.Config.LogTypes_RELEASE = Enum.GetValues(typeof(StswInfoType)).Cast<StswInfoType>();
        StswLog.Config.MaxFailures = 3;
    }

    private void Cleanup()
    {
        foreach (var f in Directory.GetFiles(_logDir)) File.Delete(f);
        foreach (var f in Directory.GetFiles(_archiveDir)) File.Delete(f);
    }

    [Fact]
    public void Write_WritesLogFile()
    {
        Cleanup();
        StswLog.Write(StswInfoType.Information, "Test log entry");
        var logFile = Directory.GetFiles(_logDir, "log_*.log").FirstOrDefault();
        Assert.NotNull(logFile);
        var content = File.ReadAllText(logFile!);
        Assert.Contains("Test log entry", content);
    }

    [Fact]
    public async Task WriteAsync_WritesLogFile()
    {
        Cleanup();
        await StswLog.WriteAsync(StswInfoType.Debug, "Async log entry");
        var logFile = Directory.GetFiles(_logDir, "log_*.log").FirstOrDefault();
        Assert.NotNull(logFile);
        var content = File.ReadAllText(logFile!);
        Assert.Contains("Async log entry", content);
    }

    [Fact]
    public void WriteException_WritesExceptionDetails()
    {
        Cleanup();
        var ex = new InvalidOperationException("Test exception");
        StswLog.WriteException(ex, StswInfoType.Error, "TestContext");
        var logFile = Directory.GetFiles(_logDir, "log_*.log").FirstOrDefault();
        Assert.NotNull(logFile);
        var content = File.ReadAllText(logFile!);
        Assert.Contains("Test exception", content);
        Assert.Contains("TestContext", content);
    }

    [Fact]
    public void WriteWithCaller_IncludesCallerInfo()
    {
        Cleanup();
        StswLog.WriteWithCaller(StswInfoType.Warning, "Caller info test");
        var logFile = Directory.GetFiles(_logDir, "log_*.log").FirstOrDefault();
        Assert.NotNull(logFile);
        var content = File.ReadAllText(logFile!);
        Assert.Contains("Caller info test", content);
        Assert.Contains("StswLogTests.cs", content);
    }

    [Fact]
    public void ImportList_ReturnsLogItems()
    {
        Cleanup();
        StswLog.Write(StswInfoType.Success, "Import test entry");
        var today = DateTime.Now.Date;
        var items = StswLog.ImportList(today, today).ToList();
        Assert.NotEmpty(items);
        Assert.Contains(items, i => i.Text != null && i.Text.Contains("Import test entry"));
    }

    [Fact]
    public async Task ImportListAsync_ReturnsLogItems()
    {
        Cleanup();
        await StswLog.WriteAsync(StswInfoType.Fatal, "Async import test");
        var today = DateTime.Now.Date;
        var items = (await StswLog.ImportListAsync(today, today)).ToList();
        Assert.NotEmpty(items);
        Assert.Contains(items, i => i.Text != null && i.Text.Contains("Async import test"));
    }

    [Fact]
    public void Archive_ArchivesLogFiles()
    {
        Cleanup();
        StswLog.Write(StswInfoType.Debug, "Archive test entry");
        var today = DateTime.Now.Date;
        StswLog.Archive(today, today);
        var archiveFile = Directory.GetFiles(_archiveDir, "archive_*.zip").FirstOrDefault();
        Assert.NotNull(archiveFile);
        using var zip = ZipFile.OpenRead(archiveFile!);
        Assert.True(zip.Entries.Any(e => e.Name.StartsWith("log_")));
    }

    [Fact]
    public void ArchiveSingleLogBySize_ArchivesWhenSizeExceeded()
    {
        Cleanup();
        var logPath = Path.Combine(_logDir, $"log_{DateTime.Now:yyyy-MM-dd}.log");
        File.WriteAllText(logPath, new string('x', 1024 * 10));
        StswLog.Config.Archive.ArchiveWhenSizeOver = 1024;
        typeof(StswLog).GetMethod("ForceSizeArchiveIfNeeded", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
            .Invoke(null, null);
        var archiveFile = Directory.GetFiles(_archiveDir, "archive_*.zip").FirstOrDefault();
        Assert.NotNull(archiveFile);
        Assert.False(File.Exists(logPath));
    }

    [Fact]
    public void DeleteOldArchives_RemovesOldArchives()
    {
        Cleanup();
        var archivePath = Path.Combine(_archiveDir, "archive_2000-01-01.zip");
        using (var zip = ZipFile.Open(archivePath, ZipArchiveMode.Create))
        {
            var entry = zip.CreateEntry("log_2000-01-01.log");
            using var stream = entry.Open();
            stream.Write(new byte[] { 1, 2, 3 });
        }
        StswLog.Config.Archive.DeleteArchivesOlderThanDays = 1;
        typeof(StswLog).GetMethod("DeleteOldArchives", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
            .Invoke(null, null);
        Assert.False(File.Exists(archivePath));
    }

    [Fact]
    public void ShouldLog_RespectsConfig()
    {
        Cleanup();
        StswLog.Config.LogTypes_DEBUG = new[] { StswInfoType.Error };
        StswLog.Config.LogTypes_RELEASE = new[] { StswInfoType.Error };
        StswLog.Write(StswInfoType.Debug, "Should not log");
        var logFile = Directory.GetFiles(_logDir, "log_*.log").FirstOrDefault();
        if (logFile != null)
        {
            var content = File.ReadAllText(logFile);
            Assert.DoesNotContain("Should not log", content);
        }
    }

    [Fact]
    public void HandleLoggingFailure_DisablesLoggingAfterMaxFailures()
    {
        Cleanup();
        StswLog.Config.MaxFailures = 2;
        int failCount = 0;
        StswLog.Config.OnLogFailure = _ => failCount++;
        // Simulate failure by making log directory unwritable
        var oldDir = StswLog.Config.LogDirectoryPath;
        StswLog.Config.LogDirectoryPath = Path.Combine(_logDir, "nonexistent", "fail");
        StswLog.Write(StswInfoType.Error, "Should fail");
        StswLog.Write(StswInfoType.Error, "Should fail again");
        Assert.True(StswLog.Config.IsLoggingDisabled);
        Assert.True(failCount >= 2);
        StswLog.Config.LogDirectoryPath = oldDir;
    }
}
