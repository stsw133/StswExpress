using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;

namespace StswExpress.Wpf;
/// <summary>
/// A base rich text container that can render content from a file path.
/// Designed to be lightweight and optionally read-only without editor decorations.
/// </summary>
public class StswRichBox : RichTextBox, IStswBoxControl, IStswCornerControl
{
    static StswRichBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswRichBox), new FrameworkPropertyMetadata(typeof(StswRichBox)));
    }
    public StswRichBox()
    {
        SetValue(SubControlsProperty, new ObservableCollection<IStswSubControl>());
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
            typeof(StswRichBox)
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
            typeof(StswRichBox)
        );

    /// <inheritdoc/>
    public ReadOnlyObservableCollection<ValidationError> Errors
    {
        get => (ReadOnlyObservableCollection<ValidationError>)GetValue(ErrorsProperty);
        set => SetValue(ErrorsProperty, value);
    }
    public static readonly DependencyProperty ErrorsProperty
        = DependencyProperty.Register(
            nameof(Errors),
            typeof(ReadOnlyObservableCollection<ValidationError>),
            typeof(StswRichBox)
        );

    /// <summary>
    /// Gets or sets the file path associated with the current document.
    /// When changed, the content of the editor is updated accordingly.
    /// </summary>
    public string? FilePath
    {
        get => (string?)GetValue(FilePathProperty);
        set => SetValue(FilePathProperty, value);
    }
    public static readonly DependencyProperty FilePathProperty
        = DependencyProperty.Register(
            nameof(FilePath),
            typeof(string),
            typeof(StswRichBox),
            new FrameworkPropertyMetadata(default(string?),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnFilePathChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    public static void OnFilePathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var stsw = (StswRichBox)d;
        stsw.LoadFilePath();
    }

    /// <summary>
    /// Gets or sets the formatted rich text content of the control in Rich Text Format (RTF).
    /// When set, the document is replaced with the provided formatted text.
    /// When retrieved, the current content of the document is returned as an RTF string.
    /// </summary>
    public string? FormattedText
    {
        get => (string?)GetValue(FormattedTextProperty);
        set => SetValue(FormattedTextProperty, value);
    }
    public static readonly DependencyProperty FormattedTextProperty
        = DependencyProperty.Register(
            nameof(FormattedText),
            typeof(string),
            typeof(StswRichBox),
            new FrameworkPropertyMetadata(default(string?),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnFormattedTextChanged, null, false, UpdateSourceTrigger.LostFocus)
        );
    public static void OnFormattedTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var stsw = (StswRichBox)d;
        if (!stsw._suppressFormattedTextUpdate)
            stsw.LoadFormattedText(e.NewValue as string);
    }

    /// <inheritdoc/>
    public bool HasError
    {
        get => (bool)GetValue(HasErrorProperty);
        set => SetValue(HasErrorProperty, value);
    }
    public static readonly DependencyProperty HasErrorProperty
        = DependencyProperty.Register(
            nameof(HasError),
            typeof(bool),
            typeof(StswRichBox)
        );

    /// <inheritdoc/>
    public object? Icon
    {
        get => (object?)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }
    public static readonly DependencyProperty IconProperty
        = DependencyProperty.Register(
            nameof(Icon),
            typeof(object),
            typeof(StswRichBox)
        );

    /// <summary>
    /// Gets or sets the collection of sub-controls associated with the editor.
    /// These can be used for adding additional UI elements like buttons or dropdowns.
    /// </summary>
    public ObservableCollection<IStswSubControl> SubControls
    {
        get => (ObservableCollection<IStswSubControl>)GetValue(SubControlsProperty);
        set => SetValue(SubControlsProperty, value);
    }
    public static readonly DependencyProperty SubControlsProperty
        = DependencyProperty.Register(
            nameof(SubControls),
            typeof(ObservableCollection<IStswSubControl>),
            typeof(StswRichBox)
        );
    #endregion

    #region Template
    /// <inheritdoc/>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        LoadFilePath();
    }
    #endregion

    #region Overrides
    /// <inheritdoc/>
    protected override void OnLostFocus(RoutedEventArgs e)
    {
        CommitFormattedText();
        base.OnLostFocus(e);
    }

    /// <inheritdoc/>
    protected override void OnTextChanged(TextChangedEventArgs e)
    {
        base.OnTextChanged(e);

        if (_suppressFormattedTextUpdate)
            return;

        _isDocumentDirty = true;
    }
    #endregion

    #region Logic
    private bool _isDocumentDirty;
    private bool _suppressFormattedTextUpdate;

    /// <summary>
    /// Commits the current document content to the <see cref="FormattedText"/> property in RTF format.
    /// </summary>
    private void CommitFormattedText()
    {
        if (_suppressFormattedTextUpdate || !_isDocumentDirty)
            return;

        _suppressFormattedTextUpdate = true;

        using var stream = new MemoryStream();
        var range = new TextRange(Document.ContentStart, Document.ContentEnd);
        range.Save(stream, DataFormats.Rtf);
        stream.Position = 0;

        using var reader = new StreamReader(stream, Encoding.UTF8);
        SetCurrentValue(FormattedTextProperty, reader.ReadToEnd());

        _isDocumentDirty = false;
        _suppressFormattedTextUpdate = false;
    }

    /// <summary>
    /// Loads content from the provided <see cref="FilePath"/> if it exists, otherwise clears the document.
    /// </summary>
    protected void LoadFilePath()
    {
        _suppressFormattedTextUpdate = true;

        if (FilePath != null && File.Exists(FilePath))
        {
            using var fileStream = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            var range = new TextRange(Document.ContentStart, Document.ContentEnd);
            range.Load(fileStream, DataFormats.Rtf);
        }
        else
        {
            Document.Blocks.Clear();
        }

        IsUndoEnabled = !IsUndoEnabled;
        IsUndoEnabled = !IsUndoEnabled;

        _suppressFormattedTextUpdate = false;

        _isDocumentDirty = true;
        CommitFormattedText();
    }

    /// <summary>
    /// Replaces the document content using the provided RTF formatted text.
    /// </summary>
    /// <param name="formattedText">The RTF string to load into the document.</param>
    private void LoadFormattedText(string? formattedText)
    {
        _suppressFormattedTextUpdate = true;

        if (!string.IsNullOrWhiteSpace(formattedText))
        {
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(formattedText));
            var range = new TextRange(Document.ContentStart, Document.ContentEnd);
            range.Load(stream, DataFormats.Rtf);
        }
        else
        {
            Document.Blocks.Clear();
        }

        IsUndoEnabled = !IsUndoEnabled;
        IsUndoEnabled = !IsUndoEnabled;

        _suppressFormattedTextUpdate = false;

        _isDocumentDirty = false;
    }
    #endregion

    #region Excluded properties
    /// The following properties are hidden from the designer and serialization:

    [Bindable(false)]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete($"{nameof(Placeholder)} is not supported in {nameof(StswRichBox)}.")]
    public string? Placeholder
    {
        get => default;
        set => throw new NotSupportedException($"{nameof(Placeholder)} is not supported in {nameof(StswRichBox)}.");
    }
    [Obsolete($"{nameof(Placeholder)} is not supported in {nameof(StswRichBox)}.")]
    public static readonly DependencyProperty PlaceholderProperty
        = DependencyProperty.Register(
            nameof(Placeholder),
            typeof(string),
            typeof(StswRichBox),
            new FrameworkPropertyMetadata(default(string?), OnPlaceholderChanged)
        );
    private static void OnPlaceholderChanged(DependencyObject _, DependencyPropertyChangedEventArgs __) => throw new NotSupportedException($"{nameof(Placeholder)} is not supported in {nameof(StswRichBox)}.");
    #endregion
}
