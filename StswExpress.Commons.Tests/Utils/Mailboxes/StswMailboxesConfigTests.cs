using System.IO;

namespace StswExpress.Commons.Tests.Utils.Mailboxes;
public class StswMailboxesConfigTests
{
    [Fact]
    public void Default_FilePath_IsCorrect()
    {
        var config = new StswMailboxesConfig();
        var expectedPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "mailboxes.stsw");
        Assert.Equal(expectedPath, config.FilePath);
    }

    [Fact]
    public void Default_IsEnabled_IsTrue()
    {
        var config = new StswMailboxesConfig();
        Assert.True(config.IsEnabled);
    }

    [Fact]
    public void DebugEmailRecipients_CanBeSetAndRetrieved()
    {
        var config = new StswMailboxesConfig();
        var recipients = new[] { "test1@example.com", "test2@example.com" };
        config.DebugEmailRecipients = recipients;
        Assert.Equal(recipients, config.DebugEmailRecipients);
    }

    [Fact]
    public void DebugEmailRecipients_DefaultIsNull()
    {
        var config = new StswMailboxesConfig();
        Assert.Null(config.DebugEmailRecipients);
    }

    [Fact]
    public void FilePath_CanBeSetAndRetrieved()
    {
        var config = new StswMailboxesConfig();
        var customPath = @"C:\custom\mailboxes.stsw";
        config.FilePath = customPath;
        Assert.Equal(customPath, config.FilePath);
    }

    [Fact]
    public void IsEnabled_CanBeSetAndRetrieved()
    {
        var config = new StswMailboxesConfig();
        config.IsEnabled = false;
        Assert.False(config.IsEnabled);
        config.IsEnabled = true;
        Assert.True(config.IsEnabled);
    }
}
