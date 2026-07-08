using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace TestApp.Wpf;
public partial class MailboxesContext : StswObservableObject
{
    public MailboxesContext()
    {
        SelectedMailbox = AllMailboxes.FirstOrDefault();
        DebugRecipientsText = string.Join("; ", StswMailboxes.Config.DebugEmailRecipients ?? []);
    }

    public bool IsMailingEnabled
    {
        get => StswMailboxes.Config.IsEnabled;
        set
        {
            if (StswMailboxes.Config.IsEnabled == value)
                return;

            StswMailboxes.Config.IsEnabled = value;
            OnPropertyChanged();
        }
    }

    [StswCommand]
    async Task MoveUp()
    {
        try
        {
            if (AllMailboxes.IndexOf(SelectedMailbox!) is int i and > 0)
                AllMailboxes.Move(i, i - 1);
        }
        catch (Exception ex)
        {
            await StswMessageDialog.Show(ex, MethodBase.GetCurrentMethod()?.Name);
        }
    }

    [StswCommand]
    async Task MoveDown()
    {
        try
        {
            if (AllMailboxes.IndexOf(SelectedMailbox!) is int i and >= 0 && i < AllMailboxes.Count - 1)
                AllMailboxes.Move(i, i + 1);
        }
        catch (Exception ex)
        {
            await StswMessageDialog.Show(ex, MethodBase.GetCurrentMethod()?.Name);
        }
    }

    [StswCommand]
    async Task Add()
    {
        try
        {
            var newMailbox = new StswMailboxModel
            {
                Name = "New mailbox",
                Port = 587,
                SecurityOption = StswMailSecurityOption.Auto
            };

            AllMailboxes.Add(newMailbox);
            SelectedMailbox = newMailbox;
        }
        catch (Exception ex)
        {
            await StswMessageDialog.Show(ex, MethodBase.GetCurrentMethod()?.Name);
        }
    }

    [StswCommand]
    async Task Remove()
    {
        try
        {
            if (SelectedMailbox != null)
                AllMailboxes.Remove(SelectedMailbox);

            SelectedMailbox = AllMailboxes.FirstOrDefault();
        }
        catch (Exception ex)
        {
            await StswMessageDialog.Show(ex, MethodBase.GetCurrentMethod()?.Name);
        }
    }

    [StswCommand]
    async Task Import()
    {
        try
        {
            AllMailboxes = new(await Task.Run(StswMailboxes.ImportList));
            SelectedMailbox = AllMailboxes.FirstOrDefault();
            StswMailboxes.Default = SelectedMailbox;
        }
        catch (Exception ex)
        {
            await StswMessageDialog.Show(ex, MethodBase.GetCurrentMethod()?.Name);
        }
    }

    [StswCommand]
    async Task Export()
    {
        try
        {
            ApplyReplyToToSelectedMailbox();
            await Task.Run(() => StswMailboxes.ExportList(AllMailboxes));
            await StswMessageDialog.Show("Mailboxes exported successfully.", nameof(TestApp.Wpf), null, StswDialogButtons.OK, StswDialogImage.Success);
        }
        catch (Exception ex)
        {
            await StswMessageDialog.Show(ex, MethodBase.GetCurrentMethod()?.Name);
        }
    }

    [StswCommand]
    async Task AddAttachments()
    {
        try
        {
            var dialog = new OpenFileDialog
            {
                Title = "Select mail attachments",
                Filter = "All files (*.*)|*.*",
                Multiselect = true
            };

            if (dialog.ShowDialog() == true)
                foreach (var fileName in dialog.FileNames.Where(File.Exists))
                    if (!Attachments.Contains(fileName, StringComparer.OrdinalIgnoreCase))
                        Attachments.Add(fileName);
        }
        catch (Exception ex)
        {
            await StswMessageDialog.Show(ex, MethodBase.GetCurrentMethod()?.Name);
        }
    }

    [StswCommand]
    void RemoveAttachment(string? filePath)
    {
        if (!string.IsNullOrWhiteSpace(filePath))
            Attachments.Remove(filePath);
    }

    [StswCommand]
    void ClearAttachments() => Attachments.Clear();

    [StswCommand]
    void ClearMessage()
    {
        To = null;
        Cc = null;
        Bcc = null;
        Subject = null;
        Body = null;
        IsBodyHtml = false;
        Attachments.Clear();
        LastStatus = null;
    }

    [StswCommand]
    async Task Send()
    {
        try
        {
            if (SelectedMailbox == null)
                throw new InvalidOperationException("Select or add a mailbox before sending an email.");

            ApplyReplyToToSelectedMailbox();

            var to = ParseAddresses(To);
            var cc = ParseAddresses(Cc);
            var bcc = ParseAddresses(Bcc);
            var attachments = Attachments.ToArray();

            if (attachments.FirstOrDefault(x => !File.Exists(x)) is string missingAttachment)
                throw new FileNotFoundException("Attachment file does not exist.", missingAttachment);

            await SelectedMailbox.SendAsync(
                to: to,
                subject: Subject ?? string.Empty,
                body: Body ?? string.Empty,
                isBodyHtml: IsBodyHtml,
                attachments: attachments,
                cc: cc,
                bcc: bcc
            );

            StswMailboxes.Default = SelectedMailbox;
            LastStatus = $"Mail sent successfully at {DateTime.Now:yyyy-MM-dd HH:mm:ss}.";
            await StswMessageDialog.Show(LastStatus, nameof(TestApp.Wpf), null, StswDialogButtons.OK, StswDialogImage.Success);
        }
        catch (Exception ex)
        {
            LastStatus = ex.Message;
            await StswMessageDialog.Show(ex, MethodBase.GetCurrentMethod()?.Name);
        }
    }

    private void ApplyReplyToToSelectedMailbox()
    {
        if (SelectedMailbox != null)
            SelectedMailbox.ReplyTo = ParseAddresses(ReplyToText);
    }

    private static string[] ParseAddresses(string? value)
        => string.IsNullOrWhiteSpace(value) ? [] : [.. StswMailAddressParser.SplitAddresses([value])];

    [StswObservableProperty] ObservableCollection<StswMailboxModel> _allMailboxes = [.. StswMailboxes.ImportList()];
    [StswObservableProperty] StswMailboxModel? _selectedMailbox;
    partial void OnSelectedMailboxChanged(StswMailboxModel? oldValue, StswMailboxModel? newValue)
    {
        ReplyToText = newValue?.ReplyTo == null ? null : string.Join("; ", newValue.ReplyTo);
        StswMailboxes.Default = newValue;
    }

    [StswObservableProperty] string? _replyToText;
    partial void OnReplyToTextChanged(string? oldValue, string? newValue) => ApplyReplyToToSelectedMailbox();

    [StswObservableProperty] string? _debugRecipientsText;
    partial void OnDebugRecipientsTextChanged(string? oldValue, string? newValue)
    {
        var recipients = ParseAddresses(newValue);
        StswMailboxes.Config.DebugEmailRecipients = recipients.Length == 0 ? null : recipients;
    }

    [StswObservableProperty] string? _to;
    [StswObservableProperty] string? _cc;
    [StswObservableProperty] string? _bcc;
    [StswObservableProperty] string? _subject;
    [StswObservableProperty] string? _body;
    [StswObservableProperty] bool _isBodyHtml;
    [StswObservableProperty] ObservableCollection<string> _attachments = [];
    [StswObservableProperty] string? _lastStatus;
}
