using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace StswExpress.Wpf;
/// <summary>
/// Represents a message dialog control that behaves like a content dialog.
/// Supports customizable title, message, details, and predefined button layouts.
/// </summary>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;se:StswMessageDialog Title="Warning" Message="Are you sure?" Buttons="YesNoCancel" IsOpen="True"/&gt;
/// </code>
/// </example>
public class StswFileDialog : Control, IStswCornerControl
{
    static StswFileDialog()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswFileDialog), new FrameworkPropertyMetadata(typeof(StswFileDialog)));
    }
    public StswFileDialog()
    {
        AcceptCommand = new StswCommand(Accept, () => SelectedPath is not null);
        CancelCommand = new StswCommand(Cancel, () => SelectedPath is not null);
        CloseCommand = new StswCommand(Close);
    }

    #region Dependency properties
    /// <inheritdoc/>
    public bool CornerClipping
    {
        get => (bool)GetValue(CornerClippingProperty);
        set => SetValue(CornerClippingProperty, value);
    }
    public static readonly DependencyProperty CornerClippingProperty
        = DependencyProperty.Register(
            nameof(CornerClipping),
            typeof(bool),
            typeof(StswFileDialog),
            new FrameworkPropertyMetadata(default(bool),
                FrameworkPropertyMetadataOptions.AffectsRender)
        );

    /// <inheritdoc/>
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }
    public static readonly DependencyProperty CornerRadiusProperty
        = DependencyProperty.Register(
            nameof(CornerRadius),
            typeof(CornerRadius),
            typeof(StswFileDialog),
            new FrameworkPropertyMetadata(default(CornerRadius),
                FrameworkPropertyMetadataOptions.AffectsRender)
        );

    /// <summary>
    /// Gets or sets the filter string that determines which file types are displayed in the dialog.
    /// </summary>
    public string Filter
    {
        get => (string)GetValue(FilterProperty);
        set => SetValue(FilterProperty, value);
    }
    public static readonly DependencyProperty FilterProperty
        = DependencyProperty.Register(
            nameof(Filter),
            typeof(string),
            typeof(StswFileDialog)
        );

    /// <summary>
    /// Gets or sets the initial path that the dialog will display when opened.
    /// </summary>
    public string? InitialPath
    {
        get => (string?)GetValue(InitialPathProperty);
        set => SetValue(InitialPathProperty, value);
    }
    public static readonly DependencyProperty InitialPathProperty
        = DependencyProperty.Register(
            nameof(InitialPath),
            typeof(string),
            typeof(StswFileDialog)
        );

    /// <summary>
    /// Gets or sets an identifier object to distinguish between multiple dialog instances.
    /// </summary>
    public object? Identifier
    {
        get => GetValue(IdentifierProperty);
        set => SetValue(IdentifierProperty, value);
    }
    public static readonly DependencyProperty IdentifierProperty
        = DependencyProperty.Register(
            nameof(Identifier),
            typeof(object),
            typeof(StswFileDialog)
        );

    /// <summary>
    /// Gets or sets a value indicating whether the file dialog is currently open.
    /// </summary>
    public bool IsOpen
    {
        get => (bool)GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }
    public static readonly DependencyProperty IsOpenProperty
        = DependencyProperty.Register(
            nameof(IsOpen),
            typeof(bool),
            typeof(StswFileDialog),
            new FrameworkPropertyMetadata(default(bool),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                null, null, false, UpdateSourceTrigger.PropertyChanged)
        );

    /// <summary>
    /// Gets or sets a value indicating whether files should be shown in the dialog.
    /// </summary>
    public bool ShowFiles
    {
        get => (bool)GetValue(ShowFilesProperty);
        set => SetValue(ShowFilesProperty, value);
    }
    public static readonly DependencyProperty ShowFilesProperty
        = DependencyProperty.Register(
            nameof(ShowFiles),
            typeof(bool),
            typeof(StswFileDialog),
            new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)
        );

    /// <summary>
    /// Gets or sets the selected file path from the dialog.
    /// </summary>
    public string? SelectedPath
    {
        get => (string?)GetValue(SelectedPathProperty);
        set => SetValue(SelectedPathProperty, value);
    }
    public static readonly DependencyProperty SelectedPathProperty
        = DependencyProperty.Register(
            nameof(SelectedPath),
            typeof(string),
            typeof(StswFileDialog),
            new FrameworkPropertyMetadata(default(string?),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                (_, __) => CommandManager.InvalidateRequerySuggested())
        );

    /// <summary>
    /// Gets or sets the title of the file dialog.
    /// </summary>
    public string? Title
    {
        get => (string?)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }
    public static readonly DependencyProperty TitleProperty
        = DependencyProperty.Register(
            nameof(Title),
            typeof(string),
            typeof(StswFileDialog)
        );
    #endregion

    #region Logic
    public ICommand AcceptCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand CloseCommand { get; }

    /// <summary>
    /// Handles the accept action by closing the dialog with the selected path.
    /// </summary>
    private void Accept() => StswContentDialog.Close(Identifier, SelectedPath);

    /// <summary>
    /// Handles the cancel action by setting the selected path to null and closing the dialog.
    /// </summary>
    private void Cancel()
    {
        SetCurrentValue(SelectedPathProperty, null);
        StswContentDialog.Close(Identifier, null);
    }

    /// <summary>
    /// Closes the message dialog and sets the result.
    /// </summary>
    private void Close() => StswContentDialog.Close(Identifier, null);

    public static async Task<string?> Show(string? initialPath = null, string filter = "", object? identifier = null)
    {
        var dialog = new StswFileDialog()
        {
            InitialPath = initialPath,
            Filter = filter,
            Identifier = identifier ?? StswApp.StswWindow
        };

        return (string?)await StswContentDialog.Show(dialog, dialog.Identifier);
    }
    #endregion
}
