using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace StswExpress.Commons.Tests.Utils.Mailboxes;
public class StswMailboxModelTests
{
    private class TestStswMailboxesConfig
    {
        public bool IsEnabled { get; set; } = true;
        public List<string>? DebugEmailRecipients { get; set; }
    }

    private class TestStswMailboxes
    {
        public static TestStswMailboxesConfig Config { get; set; } = new TestStswMailboxesConfig();
    }

    private class TestStswFn
    {
        public static bool IsInDebug { get; set; }
    }

    private StswMailboxModel CreateMailbox()
    {
        return new StswMailboxModel("smtp.test.com", 587, "from@test.com", "user", "pass")
        {
            Domain = "test.com",
            IgnoreCertificateErrors = true,
            SecurityOption = SecureSocketOptions.StartTls
        };
    }

    [Fact]
    public void Constructor_Default_SetsProperties()
    {
        var mailbox = new StswMailboxModel();
        Assert.Null(mailbox.Host);
        Assert.Null(mailbox.From);
        Assert.Null(mailbox.Username);
        Assert.Null(mailbox.Password);
        Assert.Equal(587, mailbox.Port);
    }

    [Fact]
    public void Constructor_WithParameters_SetsProperties()
    {
        var mailbox = new StswMailboxModel("smtp.test.com", 2525, "from@test.com", "user", "pass");
        Assert.Equal("smtp.test.com", mailbox.Host);
        Assert.Equal(2525, mailbox.Port);
        Assert.Equal("from@test.com", mailbox.From);
        Assert.Equal("user", mailbox.Username);
        Assert.Equal("pass", mailbox.Password);
    }

    [Fact]
    public void Properties_SetAndGet_WorkCorrectly()
    {
        var mailbox = new StswMailboxModel();
        mailbox.Name = "TestName";
        mailbox.Host = "host";
        mailbox.Port = 123;
        mailbox.From = "from";
        mailbox.Username = "user";
        mailbox.Password = "pass";
        mailbox.Domain = "domain";
        mailbox.ReplyTo = ["reply@test.com"];
        mailbox.IgnoreCertificateErrors = true;
        mailbox.SecurityOption = SecureSocketOptions.SslOnConnect;

        Assert.Equal("TestName", mailbox.Name);
        Assert.Equal("host", mailbox.Host);
        Assert.Equal(123, mailbox.Port);
        Assert.Equal("from", mailbox.From);
        Assert.Equal("user", mailbox.Username);
        Assert.Equal("pass", mailbox.Password);
        Assert.Equal("domain", mailbox.Domain);
        Assert.Contains("reply@test.com", mailbox.ReplyTo!);
        Assert.True(mailbox.IgnoreCertificateErrors);
        Assert.Equal(SecureSocketOptions.SslOnConnect, mailbox.SecurityOption);
    }

    [Fact]
    public void Send_ThrowsArgumentException_WhenHostOrFromIsNull()
    {
        var mailbox = new StswMailboxModel();
        Assert.Throws<ArgumentException>(() => mailbox.Send(["to@test.com"], "subject", "body"));
        mailbox.Host = "smtp.test.com";
        Assert.Throws<ArgumentException>(() => mailbox.Send(["to@test.com"], "subject", "body"));
        mailbox.From = "from@test.com";
        mailbox.Port = null;
        Assert.Throws<ArgumentNullException>(() => mailbox.Send(["to@test.com"], "subject", "body"));
    }

    [Fact]
    public void BuildMessage_CreatesMimeMessage_WithCorrectFields()
    {
        var mailbox = CreateMailbox();
        var to = new[] { "to@test.com" };
        var cc = new[] { "cc@test.com" };
        var bcc = new[] { "bcc@test.com" };
        var replyTo = new[] { "reply@test.com" };
        mailbox.ReplyTo = replyTo;
        var subject = "Test Subject";
        var body = "<b>Test Body</b>";
        var attachments = new[] { "file.txt" };

        var message = typeof(StswMailboxModel)
            .GetMethod("BuildMessage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .Invoke(mailbox, [to, subject, body, true, attachments, cc, bcc]) as MimeMessage;

        Assert.NotNull(message);
        Assert.Equal(subject, message!.Subject);
        Assert.Contains("to@test.com", message.To.ToString());
        Assert.Contains("cc@test.com", message.Cc.ToString());
        Assert.Contains("bcc@test.com", message.Bcc.ToString());
        Assert.Contains("reply@test.com", message.ReplyTo.ToString());
        Assert.Equal("from@test.com", message.From.ToString());
        Assert.NotNull(message.Body);
    }

    [Fact]
    public void CreateConfiguredClient_ReturnsSmtpClient_WithCertificateValidation()
    {
        var mailbox = CreateMailbox();
        var client = typeof(StswMailboxModel)
            .GetMethod("CreateConfiguredClient", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .Invoke(mailbox, []) as SmtpClient;

        Assert.NotNull(client);
        Assert.NotNull(client!.ServerCertificateValidationCallback);
    }

    [Fact]
    public async Task SendAsync_ThrowsArgumentException_WhenHostOrFromIsNull()
    {
        var mailbox = new StswMailboxModel();
        await Assert.ThrowsAsync<ArgumentException>(() => mailbox.SendAsync(["to@test.com"], "subject", "body"));
        mailbox.Host = "smtp.test.com";
        await Assert.ThrowsAsync<ArgumentException>(() => mailbox.SendAsync(["to@test.com"], "subject", "body"));
        mailbox.From = "from@test.com";
        mailbox.Port = null;
        await Assert.ThrowsAsync<ArgumentNullException>(() => mailbox.SendAsync(["to@test.com"], "subject", "body"));
    }
}
