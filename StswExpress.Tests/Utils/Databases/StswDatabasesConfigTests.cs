using System.IO;

namespace StswExpress.Commons.Tests.Utils.Databases;
public class StswDatabasesConfigTests
{
    [Fact]
    public void DefaultValues_AreSetCorrectly()
    {
        var config = new StswDatabasesConfig();

        Assert.True(config.AutoDisposeConnection);
        Assert.Equal('/', config.DelimiterForMapping);
        Assert.Equal(
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "databases.stsw"),
            config.FilePath
        );
        Assert.True(config.IsEnabled);
        Assert.True(config.MakeLessSpaceQuery);
        Assert.True(config.ReturnIfInDesignMode);
    }

    [Fact]
    public void CanSetProperties()
    {
        var config = new StswDatabasesConfig
        {
            AutoDisposeConnection = false,
            DelimiterForMapping = '|',
            FilePath = "custom/path/file.stsw",
            IsEnabled = false,
            MakeLessSpaceQuery = false,
            ReturnIfInDesignMode = false
        };

        Assert.False(config.AutoDisposeConnection);
        Assert.Equal('|', config.DelimiterForMapping);
        Assert.Equal("custom/path/file.stsw", config.FilePath);
        Assert.False(config.IsEnabled);
        Assert.False(config.MakeLessSpaceQuery);
        Assert.False(config.ReturnIfInDesignMode);
    }
}
