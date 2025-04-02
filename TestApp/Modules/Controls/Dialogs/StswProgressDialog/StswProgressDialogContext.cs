namespace TestApp;

public class StswProgressDialogContext : ControlsContext
{
    public StswCommand OpenProgressDialogCommand => new(OpenProgressDialog);

    public override void SetDefaults()
    {
        base.SetDefaults();
    }

    #region Events & methods
    /// Command: open progress dialog
    private async void OpenProgressDialog()
    {
        //var result = await StswProgressDialog.Show(
        //    "Lorem ipsum dolor sit amet...",
        //    nameof(StswProgressDialogView));
        //
        //ProgressDialogResult = result?.ToString();
    }
    #endregion

    /// ProgressDialogResult
    public string? ProgressDialogResult
    {
        get => _progressDialogResult;
        set => SetProperty(ref _progressDialogResult, value);
    }
    private string? _progressDialogResult;
}
