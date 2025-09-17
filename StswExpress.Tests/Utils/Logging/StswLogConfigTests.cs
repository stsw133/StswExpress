using StswExpress.Commons;
using System;
using System.IO;
using System.Linq;

namespace StswExpress.Tests;
public class StswLogConfigTests
{
    [Fact]
    public void Default_LogDirectoryPath_IsSet()
    {
        var config = new StswLogConfig();
        Assert.Equal(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs"), config.LogDirectoryPath);
    }

    [Fact]
    public void Default_LogTypes_DEBUG_ContainsAllTypes()
    {
        var config = new StswLogConfig();
        var allTypes = Enum.GetValues(typeof(StswInfoType)).Cast<StswInfoType>();
        Assert.True(allTypes.SequenceEqual(config.LogTypes_DEBUG));
    }

    [Fact]
    public void Default_LogTypes_RELEASE_ExcludesDebug()
    {
        var config = new StswLogConfig();
        Assert.DoesNotContain(StswInfoType.Debug, config.LogTypes_RELEASE);
        Assert.Contains(StswInfoType.Error, config.LogTypes_RELEASE);
    }

    [Fact]
    public void Default_IsLoggingDisabled_IsFalse()
    {
        var config = new StswLogConfig();
        Assert.False(config.IsLoggingDisabled);
    }

    [Fact]
    public void Default_IsSqlLoggingDisabled_IsFalse()
    {
        var config = new StswLogConfig();
        Assert.False(config.IsSqlLoggingDisabled);
    }

    [Fact]
    public void Default_MaxFailures_IsThree()
    {
        var config = new StswLogConfig();
        Assert.Equal(3, config.MaxFailures);
    }

    [Fact]
    public void OnLogFailure_CanBeSetAndInvoked()
    {
        var config = new StswLogConfig();
        Exception? received = null;
        config.OnLogFailure = ex => received = ex;
        var testEx = new InvalidOperationException();
        config.OnLogFailure?.Invoke(testEx);
        Assert.Equal(testEx, received);
    }

    [Fact]
    public void SqlLogger_CanBeSetAndInvoked()
    {
        var config = new StswLogConfig();
        StswLogItem? received = null;
        config.SqlLogger = item => received = item;
        var testItem = new StswLogItem(StswInfoType.Error, "Test", DateTime.Now);
        config.SqlLogger?.Invoke(testItem);
        Assert.Equal(testItem, received);
    }

    [Fact]
    public void ArchiveConfig_Defaults_AreSet()
    {
        var archive = new StswLogConfig.StswLogArchiveConfig();
        Assert.True(archive.ArchiveFullMonth);
        Assert.Equal(90, archive.ArchiveUpToLastDays);
        Assert.Equal(120, archive.ArchiveWhenDaysOver);
        Assert.Equal(1024 * 1024 * 5, archive.ArchiveWhenSizeOver);
        Assert.Equal(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", "archive"), archive.ArchiveDirectoryPath);
        Assert.Null(archive.DeleteArchivesOlderThanDays);
    }

    [Fact]
    public void ArchiveConfig_Properties_CanBeSet()
    {
        var archive = new StswLogConfig.StswLogArchiveConfig
        {
            ArchiveFullMonth = false,
            ArchiveUpToLastDays = 10,
            ArchiveWhenDaysOver = 20,
            ArchiveWhenSizeOver = 100,
            ArchiveDirectoryPath = "custom/path",
            DeleteArchivesOlderThanDays = 365
        };
        Assert.False(archive.ArchiveFullMonth);
        Assert.Equal(10, archive.ArchiveUpToLastDays);
        Assert.Equal(20, archive.ArchiveWhenDaysOver);
        Assert.Equal(100, archive.ArchiveWhenSizeOver);
        Assert.Equal("custom/path", archive.ArchiveDirectoryPath);
        Assert.Equal(365, archive.DeleteArchivesOlderThanDays);
    }
}
