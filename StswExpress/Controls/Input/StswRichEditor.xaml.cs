using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace StswExpress;
/// <summary>
/// A rich text editor control for creating and formatting text content.
/// Supports file operations (open, save, print), text styling, and color selection.
/// </summary>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;se:StswRichEditor FilePath="C:\Documents\sample.rtf" ToolbarMode="Compact"/&gt;
/// </code>
/// </example>
[StswPlannedChanges(StswPlannedChanges.Rework, "Current implementation is obsolete and will be reworked in future versions.")]
public class StswRichEditor : RichTextBox, /*IStswBoxControl,*/ IStswCornerControl
{
    private StswComboBox? _fontFamily;
    private StswDecimalBox? _fontSize;

    public ICommand FileNewCommand { get; }
    public ICommand FileOpenCommand { get; }
    public ICommand FileSaveCommand { get; }
    public ICommand FileSaveAsCommand { get; }
    public ICommand FileReloadCommand { get; }
    public ICommand FilePrintCommand { get; }
    public ICommand FileMailCommand { get; }
    public ICommand FileInfoCommand { get; }
    public ICommand FontStrikethroughCommand { get; }
    public ICommand FontColorTextCommand { get; }
    public ICommand FontColorHighlightCommand { get; }
    public ICommand SectionInterlineCommand { get; }

    public StswRichEditor()
    {
        SetValue(SubControlsProperty, new ObservableCollection<IStswSubControl>());

        FileNewCommand = new StswCommand(FileNew);
        FileOpenCommand = new StswCommand(FileOpen);
        FileSaveCommand = new StswCommand(FileSave);
        FileSaveAsCommand = new StswCommand(FileSaveAs);
        FileReloadCommand = new StswCommand(FileReload, FileReloadCondition);
        FilePrintCommand = new StswCommand(FilePrint);
        FileMailCommand = new StswCommand(FileMail);
        FileInfoCommand = new StswCommand(FileInfo, FileInfoCondition);
        FontStrikethroughCommand = new StswCommand(FontStrikethrough, FontStrikethroughCondition);
        FontColorTextCommand = new StswCommand(FontColorText);
        FontColorHighlightCommand = new StswCommand(FontColorHighlight);
        SectionInterlineCommand = new StswCommand<object?>(SectionInterline);
    }
    static StswRichEditor()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswRichEditor), new FrameworkPropertyMetadata(typeof(StswRichEditor)));
    }

    #region Events & methods
    /// <inheritdoc/>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (_fontFamily != null)
            _fontFamily.SelectionChanged -= PART_FontFamily_SelectionChanged;
        //if (_fontSize != null)
        //    _fontSize.ValueChanged -= PART_FontSize_ValueChanged;

        /// Box: font families
        _fontFamily = GetTemplateChild("PART_FontFamily") as StswComboBox;
        if (_fontFamily != null)
        {
            _fontFamily.ItemsSource = Fonts.SystemFontFamilies.OrderBy(x => x.Source);
            _fontFamily.SelectionChanged += PART_FontFamily_SelectionChanged;
        }

        /// Box: font size
        _fontSize = GetTemplateChild("PART_FontSize") as StswDecimalBox;
        //if (_fontSize != null)
        //    _fontSize.ValueChanged += PART_FontSize_ValueChanged;

        LoadFilePath();
        //((Paragraph)Document.Blocks.FirstBlock).LineHeight = 0.0034;
    }

    /// <inheritdoc/>
    protected override void OnSelectionChanged(RoutedEventArgs e)
    {
        base.OnSelectionChanged(e);

        object? temp;
        /*
        var temp = Selection.GetPropertyValue(TextElement.FontWeightProperty);
        if (partFontBold != null)
            partFontBold.IsChecked = (temp != DependencyProperty.UnsetValue) && (temp.Equals(FontWeights.Bold));

        temp = Selection.GetPropertyValue(TextElement.FontStyleProperty);
        if (partFontItalic != null)
            partFontItalic.IsChecked = (temp != DependencyProperty.UnsetValue) && (temp.Equals(FontStyles.Italic));

        temp = Selection.GetPropertyValue(TextElement.TextDecorationsProperty);
        if (partFontUnderline != null)
            partFontUnderline.IsChecked = (temp != DependencyProperty.UnsetValue) && (temp.Equals(TextDecorations.Underline));
        */
        temp = Selection.GetPropertyValue(TextElement.FontFamilyProperty);
        if (_fontFamily != null)
            _fontFamily.SelectedItem = temp;

        temp = Selection.GetPropertyValue(TextElement.FontSizeProperty);
        if (_fontSize != null)
            _fontSize.Value = temp != DependencyProperty.UnsetValue ? Convert.ToDecimal(temp) : null;
    }

    /// <summary>
    /// Determines whether the content of the editor has been modified
    /// since the last save or load operation.
    /// </summary>
    /// <returns><see langword="true"/> if there are unsaved changes, otherwise <see langword="false"/>.</returns>
    private bool HasChanges() => CanUndo || CanRedo;

    /// <summary>
    /// Loads content from the provided <see cref="FilePath"/> if it exists, otherwise clears the document.
    /// </summary>
    protected virtual void LoadFilePath()
    {
        if (FilePath != null)
        {
            if (File.Exists(FilePath))
            {
                using var fileStream = new FileStream(FilePath, FileMode.Open);
                var range = new TextRange(Document.ContentStart, Document.ContentEnd);
                range.Load(fileStream, DataFormats.Rtf);

                IsUndoEnabled = !IsUndoEnabled;
                IsUndoEnabled = !IsUndoEnabled;
            }
        }
        else
        {
            Document.Blocks.Clear();

            IsUndoEnabled = !IsUndoEnabled;
            IsUndoEnabled = !IsUndoEnabled;
        }
    }

    /// <summary>
    /// Creates a new empty document in the editor.
    /// If there are unsaved changes, prompts the user for confirmation before clearing the content.
    /// </summary>
    private async void FileNew()
    {
        if (HasChanges())
        {
            var result = await StswMessageDialog.Show(
                StswTranslator.GetTranslation("StswRichEditor.File.New.StswMessageDialog"),
                StswTranslator.GetTranslation("StswRichEditor"),
                null,
                StswDialogButtons.YesNo,
                StswDialogImage.Question,
                options: new StswMessageDialogShowOptions { SaveLog = false });

            if (result != true)
                return;
        }

        if (FilePath != null)
            FilePath = null;
        else
            OnFilePathChanged(this, new DependencyPropertyChangedEventArgs());
    }

    /// <summary>
    /// Opens a file dialog to allow the user to load a Rich Text Format (.rtf) file.
    /// If there are unsaved changes, prompts the user before replacing the current content.
    /// </summary>
    private async void FileOpen()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Rich Text Format (*.rtf)|*.rtf|All files (*.*)|*.*"
        };
        if (dialog.ShowDialog() == true)
        {
            if (HasChanges())
            {
                var result = await StswMessageDialog.Show(
                    StswTranslator.GetTranslation("StswRichEditor.File.Open.StswMessageDialog"),
                    StswTranslator.GetTranslation("StswRichEditor"),
                    null,
                    StswDialogButtons.YesNo,
                    StswDialogImage.Question,
                    options: new StswMessageDialogShowOptions { SaveLog = false });

                if (result != true)
                    return;
            }

            if (FilePath != dialog.FileName)
                FilePath = dialog.FileName;
            else
                OnFilePathChanged(this, new DependencyPropertyChangedEventArgs());
        }
    }

    /// <summary>
    /// Saves the current document.
    /// If no file path is set, prompts the user to specify a location.
    /// </summary>
    private void FileSave()
    {
        if (!string.IsNullOrEmpty(FilePath))
        {
            using var fileStream = new FileStream(FilePath, FileMode.Create);
            var range = new TextRange(Document.ContentStart, Document.ContentEnd);
            range.Save(fileStream, DataFormats.Rtf);
        }
        else FileSaveAs();
    }

    /// <summary>
    /// Opens a "Save As" dialog, allowing the user to save the document
    /// under a new file name in Rich Text Format (.rtf).
    /// </summary>
    private void FileSaveAs()
    {
        var dialog = new SaveFileDialog
        {
            Filter = "Rich Text Format (*.rtf)|*.rtf|All files (*.*)|*.*"
        };
        if (dialog.ShowDialog() == true)
        {
            using var fileStream = new FileStream(dialog.FileName, FileMode.Create);
            var range = new TextRange(Document.ContentStart, Document.ContentEnd);
            range.Save(fileStream, DataFormats.Rtf);
        }
    }

    /// <summary>
    /// Reloads the document from its associated file path.
    /// If there are unsaved changes, prompts the user for confirmation before reloading.
    /// </summary>
    private async void FileReload()
    {
        if (HasChanges())
        {
            var result = await StswMessageDialog.Show(
                StswTranslator.GetTranslation("StswRichEditor.File.Reload.StswMessageDialog"),
                StswTranslator.GetTranslation("StswRichEditor"),
                null,
                StswDialogButtons.YesNo,
                StswDialogImage.Question,
                options: new StswMessageDialogShowOptions { SaveLog = false });

            if (result != true)
                return;
        }

        OnFilePathChanged(this, new DependencyPropertyChangedEventArgs());
    }
    private bool FileReloadCondition() => FilePath != null;

    /// <summary>
    /// Opens the Windows Explorer and selects the currently associated file in the file system.
    /// </summary>
    private void FileInfo()
    {
        if (File.Exists(FilePath))
            Process.Start("explorer.exe", "/select, \"" + FilePath + "\"");
    }
    private bool FileInfoCondition() => FilePath != null;

    /// <summary>
    /// Opens the system print dialog and prints the current content of the editor.
    /// </summary>
    private void FilePrint()
    {
        UpdateLayout();

        var printDialog = new PrintDialog();
        if (printDialog.ShowDialog() == true)
        {
            using var stream = new MemoryStream();
            var sourceDocument = new TextRange(Document.ContentStart, Document.ContentEnd);
            sourceDocument.Save(stream, DataFormats.Xaml);

            var flowDocumentCopy = new FlowDocument();
            var copyDocumentRange = new TextRange(flowDocumentCopy.ContentStart, flowDocumentCopy.ContentEnd);
            copyDocumentRange.Load(stream, DataFormats.Xaml);

            printDialog.PrintDocument(((IDocumentPaginatorSource)flowDocumentCopy).DocumentPaginator, "Printing Richtextbox Content");
        }

        UpdateLayout();
    }

    /// <summary>
    /// Opens the default email client with the editor's content as the email body,
    /// allowing the user to send the text as an email.
    /// </summary>
    private void FileMail()
    {
        string emailTo = string.Empty;
        string emailBody = Uri.EscapeDataString(new TextRange(Document.ContentStart, Document.ContentEnd).Text);

        string mailtoUri = $"mailto:{emailTo}?body={emailBody}";

        var startInfo = new ProcessStartInfo
        {
            FileName = mailtoUri,
            UseShellExecute = true
        };
        Process.Start(startInfo);
    }

    /// <summary>
    /// Event handler for the selection changed event of the font family combo box.
    /// Applies the selected font family to the text of the selected portion in the editor.
    /// </summary>
    /// <param name="sender">The sender object triggering the event</param>
    /// <param name="e">The event arguments</param>
    private void PART_FontFamily_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!Selection.IsEmpty && _fontFamily?.SelectedItem != null)
            Selection.ApplyPropertyValue(TextElement.FontFamilyProperty, _fontFamily.SelectedItem);
        Focus();
    }

    /// <summary>
    /// Event handler for the value changed event of the font size numeric box.
    /// Applies the selected font size to the text of the selected portion in the editor.
    /// </summary>
    /// <param name="sender">The sender object triggering the event</param>
    /// <param name="e">The event arguments</param>
    private void PART_FontSize_ValueChanged(object? sender, EventArgs e)
    {
        if (!Selection.IsEmpty && _fontSize?.Value != null)
            Selection.ApplyPropertyValue(TextElement.FontSizeProperty, Convert.ToDouble(_fontSize.Value));
        Focus();
    }

    /// <summary>
    /// Toggles strikethrough formatting for the selected text.
    /// </summary>
    private void FontStrikethrough()
    {
        var newTextDecoration = TextDecorations.Strikethrough;
        if (Selection.GetPropertyValue(Inline.TextDecorationsProperty) is TextDecorationCollection current && current == TextDecorations.Strikethrough)
            newTextDecoration = [];

        Selection.ApplyPropertyValue(Inline.TextDecorationsProperty, newTextDecoration);
    }
    private bool FontStrikethroughCondition() => EditingCommands.ToggleUnderline.CanExecute(null, this);

    /// <summary>
    /// Applies the selected foreground color to the currently selected text in the editor.
    /// </summary>
    private void FontColorText() => Selection.ApplyPropertyValue(Inline.ForegroundProperty, new SolidColorBrush(SelectedColorText));

    /// <summary>
    /// Applies the selected background highlight color to the currently selected text.
    /// </summary>
    private void FontColorHighlight() => Selection.ApplyPropertyValue(Inline.BackgroundProperty, new SolidColorBrush(SelectedColorHighlight));

    /// <summary>
    /// Adjusts the interline spacing for the selected paragraph(s) in the editor.
    /// </summary>
    /// <param name="parameter">The desired spacing value. If null, defaults to standard spacing.</param>
    private void SectionInterline(object? parameter)
    {
        parameter ??= double.NaN;

        var curCaret = CaretPosition;
        var curBlocks = Document.Blocks.Where(block => block.ContentEnd.CompareTo(Selection.Start) > 0 && block.ContentStart.CompareTo(Selection.End) < 0);
        foreach (var curBlock in curBlocks)
            curBlock.Margin = new Thickness(0, 0, 0, Convert.ToDouble(parameter));
    }
    #endregion

    #region Logic properties
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
            typeof(StswRichEditor),
            new FrameworkPropertyMetadata(default(string?),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnFilePathChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    public static void OnFilePathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not StswRichEditor stsw)
            return;

        stsw.LoadFilePath();
    }

    /// <summary>
    /// Gets or sets the currently selected text color in the editor.
    /// Changing this property applies the color to the selected text.
    /// </summary>
    public Color SelectedColorText
    {
        get => (Color)GetValue(SelectedColorTextProperty);
        internal set => SetValue(SelectedColorTextProperty, value);
    }
    public static readonly DependencyProperty SelectedColorTextProperty
        = DependencyProperty.Register(
            nameof(SelectedColorText),
            typeof(Color),
            typeof(StswRichEditor),
            new FrameworkPropertyMetadata(default(Color),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnSelectedColorTextChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    public static void OnSelectedColorTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not StswRichEditor stsw)
            return;

        stsw.FontColorText();
    }

    /// <summary>
    /// Gets or sets the currently selected highlight color in the editor.
    /// Changing this property applies the highlight to the selected text.
    /// </summary>
    public Color SelectedColorHighlight
    {
        get => (Color)GetValue(SelectedColorHighlightProperty);
        internal set => SetValue(SelectedColorHighlightProperty, value);
    }
    public static readonly DependencyProperty SelectedColorHighlightProperty
        = DependencyProperty.Register(
            nameof(SelectedColorHighlight),
            typeof(Color),
            typeof(StswRichEditor),
            new FrameworkPropertyMetadata(default(Color),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnSelectedColorHighlightChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    public static void OnSelectedColorHighlightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not StswRichEditor stsw)
            return;

        stsw.FontColorHighlight();
    }

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
            typeof(StswRichEditor)
        );

    /// <summary>
    /// Gets or sets the visibility and number of options displayed in the toolbar.
    /// </summary>
    public StswCompactibility ToolbarMode
    {
        get => (StswCompactibility)GetValue(ToolbarModeProperty);
        set => SetValue(ToolbarModeProperty, value);
    }
    public static readonly DependencyProperty ToolbarModeProperty
        = DependencyProperty.Register(
            nameof(ToolbarMode),
            typeof(StswCompactibility),
            typeof(StswRichEditor)
        );
    #endregion

    #region Style properties
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
            typeof(StswRichEditor),
            new FrameworkPropertyMetadata(default(bool), FrameworkPropertyMetadataOptions.AffectsRender)
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
            typeof(StswRichEditor),
            new FrameworkPropertyMetadata(default(CornerRadius), FrameworkPropertyMetadataOptions.AffectsRender)
        );

    /// <summary>
    /// Gets or sets the thickness of the separator between the main editor and its sub-controls.
    /// </summary>
    public double SeparatorThickness
    {
        get => (double)GetValue(SeparatorThicknessProperty);
        set => SetValue(SeparatorThicknessProperty, value);
    }
    public static readonly DependencyProperty SeparatorThicknessProperty
        = DependencyProperty.Register(
            nameof(SeparatorThickness),
            typeof(double),
            typeof(StswRichEditor),
            new FrameworkPropertyMetadata(default(double), FrameworkPropertyMetadataOptions.AffectsRender)
        );

    /// <summary>
    /// Gets or sets the border thickness of buttons and controls within the editor.
    /// </summary>
    public Thickness SubBorderThickness
    {
        get => (Thickness)GetValue(SubBorderThicknessProperty);
        set => SetValue(SubBorderThicknessProperty, value);
    }
    public static readonly DependencyProperty SubBorderThicknessProperty
        = DependencyProperty.Register(
            nameof(SubBorderThickness),
            typeof(Thickness),
            typeof(StswRichEditor),
            new FrameworkPropertyMetadata(default(Thickness), FrameworkPropertyMetadataOptions.AffectsRender)
        );

    /// <summary>
    /// Gets or sets the corner radius for buttons and additional UI elements in the editor.
    /// </summary>
    public CornerRadius SubCornerRadius
    {
        get => (CornerRadius)GetValue(SubCornerRadiusProperty);
        set => SetValue(SubCornerRadiusProperty, value);
    }
    public static readonly DependencyProperty SubCornerRadiusProperty
        = DependencyProperty.Register(
            nameof(SubCornerRadius),
            typeof(CornerRadius),
            typeof(StswRichEditor),
            new FrameworkPropertyMetadata(default(CornerRadius), FrameworkPropertyMetadataOptions.AffectsRender)
        );

    /// <summary>
    /// Gets or sets the padding applied to buttons and other UI elements inside the editor.
    /// </summary>
    public Thickness SubPadding
    {
        get => (Thickness)GetValue(SubPaddingProperty);
        set => SetValue(SubPaddingProperty, value);
    }
    public static readonly DependencyProperty SubPaddingProperty
        = DependencyProperty.Register(
            nameof(SubPadding),
            typeof(Thickness),
            typeof(StswRichEditor),
            new FrameworkPropertyMetadata(default(Thickness), FrameworkPropertyMetadataOptions.AffectsMeasure)
        );
    #endregion
}
