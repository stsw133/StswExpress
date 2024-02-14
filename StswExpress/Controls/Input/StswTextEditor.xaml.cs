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
/// A custom rich text editor control that extends the functionality of the built-in <see cref="RichTextBox"/>.
/// </summary>
public class StswTextEditor : RichTextBox, /*IStswBoxControl,*/ IStswCornerControl
{
    public ICommand FileNewCommand { get; set; }
    public ICommand FileOpenCommand { get; set; }
    public ICommand FileSaveCommand { get; set; }
    public ICommand FileSaveAsCommand { get; set; }
    public ICommand FileReloadCommand { get; set; }
    public ICommand FilePrintCommand { get; set; }
    public ICommand FileMailCommand { get; set; }
    public ICommand FileInfoCommand { get; set; }
    public ICommand FontStrikethroughCommand { get; set; }
    public ICommand FontColorTextCommand { get; set; }
    public ICommand FontColorHighlightCommand { get; set; }
    public ICommand SectionInterlineCommand { get; set; }

    public StswTextEditor()
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
    static StswTextEditor()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswTextEditor), new FrameworkPropertyMetadata(typeof(StswTextEditor)));
    }

    #region Events & methods
    private StswComboBox? _fontFamily;
    private StswNumericBox? _fontSize;

    /// <summary>
    /// Occurs when the template is applied to the control.
    /// </summary>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        /// Box: font families
        if (GetTemplateChild("PART_FontFamily") is StswComboBox fontFamily)
        {
            fontFamily.ItemsSource = Fonts.SystemFontFamilies.OrderBy(x => x.Source);
            fontFamily.SelectionChanged += PART_FontFamily_SelectionChanged;
            _fontFamily = fontFamily;
        }
        /// Box: font size
        if (GetTemplateChild("PART_FontSize") is StswNumericBox fontSize)
        {
            fontSize.ValueChanged += PART_FontSize_ValueChanged;
            _fontSize = fontSize;
        }

        OnFilePathChanged(this, new DependencyPropertyChangedEventArgs());
        //((Paragraph)Document.Blocks.FirstBlock).LineHeight = 0.0034;
    }

    /// <summary>
    /// Occurs when the editor's selection changes.
    /// </summary>
    /// <param name="e">The event arguments</param>
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
    /// Checks whether there are any changes in the editor's content compared to the original content loaded from a file.
    /// </summary>
    private bool HasChanges() => CanUndo || CanRedo;

    /// Command: new
    /// <summary>
    /// Creates a new empty document in the editor, discarding any unsaved changes if present.
    /// </summary>
    private async void FileNew()
    {
        if (HasChanges())
        {
            var result = await StswMessageDialog.Show(StswTranslator.Tr("StswTextEditor.File.New.StswMessageDialog"), StswTranslator.Tr("StswTextEditor"), StswDialogButtons.YesNo, StswDialogImage.Question);
            if (result != true)
                return;
        }

        if (FilePath != null)
            FilePath = null;
        else
            OnFilePathChanged(this, new DependencyPropertyChangedEventArgs());
    }

    /// Command: open
    /// <summary>
    /// Opens a file dialog to load a Rich Text Format (.rtf) file into the editor, discarding any unsaved changes if present.
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
                var result = await StswMessageDialog.Show(StswTranslator.Tr("StswTextEditor.File.Open.StswMessageDialog"), StswTranslator.Tr("StswTextEditor"), StswDialogButtons.YesNo, StswDialogImage.Question);
                if (result != true)
                    return;
            }

            if (FilePath != dialog.FileName)
                FilePath = dialog.FileName;
            else
                OnFilePathChanged(this, new DependencyPropertyChangedEventArgs());
        }
    }

    /// Command: save
    /// <summary>
    /// Saves the content of the editor to the associated file path, or prompts the user to choose a file if the path is not set.
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

    /// Command: save as
    /// <summary>
    /// Prompts the user to choose a file path and saves the content of the editor to the selected location in Rich Text Format (.rtf).
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

    /// Command: reload
    /// <summary>
    /// Reloads the content of the editor from the associated file path, discarding any unsaved changes if present.
    /// </summary>
    private async void FileReload()
    {
        if (HasChanges())
        {
            var result = await StswMessageDialog.Show(StswTranslator.Tr("StswTextEditor.File.Reload.StswMessageDialog"), StswTranslator.Tr("StswTextEditor"), StswDialogButtons.YesNo, StswDialogImage.Question);
            if (result != true)
                return;
        }

        OnFilePathChanged(this, new DependencyPropertyChangedEventArgs());
    }
    private bool FileReloadCondition() => FilePath != null;

    /// Command: info
    /// <summary>
    /// Opens the file explorer with the currently associated file selected, if available.
    /// </summary>
    private void FileInfo()
    {
        if (File.Exists(FilePath))
            Process.Start("explorer.exe", "/select, \"" + FilePath + "\"");
    }
    private bool FileInfoCondition() => FilePath != null;

    /// Command: print
    /// <summary>
    /// Prints the content of the editor using the system's default print dialog.
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

    /// Command: mail
    /// <summary>
    /// Opens the default email client with the editor's content as the email body for sharing.
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

    /// Command: strikethrough
    /// <summary>
    /// Toggles the strikethrough formatting for the selected text.
    /// </summary>
    private void FontStrikethrough()
    {
        var newTextDecoration = TextDecorations.Strikethrough;
        if (Selection.GetPropertyValue(Inline.TextDecorationsProperty) is TextDecorationCollection current && current == TextDecorations.Strikethrough)
            newTextDecoration = new TextDecorationCollection();

        Selection.ApplyPropertyValue(Inline.TextDecorationsProperty, newTextDecoration);
    }
    private bool FontStrikethroughCondition() => EditingCommands.ToggleUnderline.CanExecute(null, this);

    /// Command: color
    /// <summary>
    /// Applies the selected color to the text of the selected portion in the editor.
    /// </summary>
    private void FontColorText() => Selection.ApplyPropertyValue(Inline.ForegroundProperty, new SolidColorBrush(SelectedColorText));

    /// Command: highlight
    /// <summary>
    /// Applies the selected color as a highlight to the selected portion in the editor.
    /// </summary>
    private void FontColorHighlight() => Selection.ApplyPropertyValue(Inline.BackgroundProperty, new SolidColorBrush(SelectedColorHighlight));

    /// Command: interline
    /// <summary>
    /// Adjusts the interline spacing for the selected paragraph(s) in the editor.
    /// </summary>
    private void SectionInterline(object? parameter)
    {
        parameter ??= double.NaN;

        var curCaret = CaretPosition;
        var curBlocks = Document.Blocks.Where(block => block.ContentEnd.CompareTo(Selection.Start) > 0 && block.ContentStart.CompareTo(Selection.End) < 0);
        foreach (var curBlock in curBlocks)
            curBlock.Margin = new Thickness(0, 0, 0, Convert.ToDouble(parameter));
    }
    #endregion

    #region Main properties
    /// <summary>
    /// Gets or sets the file path associated with the content of the editor.
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
            typeof(StswTextEditor),
            new FrameworkPropertyMetadata(default(string?),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnFilePathChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    public static void OnFilePathChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswTextEditor stsw)
        {
            if (stsw.FilePath != null)
            {
                if (File.Exists(stsw.FilePath))
                {
                    using var fileStream = new FileStream(stsw.FilePath, FileMode.Open);
                    var range = new TextRange(stsw.Document.ContentStart, stsw.Document.ContentEnd);
                    range.Load(fileStream, DataFormats.Rtf);

                    stsw.IsUndoEnabled = !stsw.IsUndoEnabled;
                    stsw.IsUndoEnabled = !stsw.IsUndoEnabled;
                }
            }
            else
            {
                stsw.Document.Blocks.Clear();

                stsw.IsUndoEnabled = !stsw.IsUndoEnabled;
                stsw.IsUndoEnabled = !stsw.IsUndoEnabled;
            }
        }
    }

    /// <summary>
    /// Gets or sets the selected text color in the editor.
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
            typeof(StswTextEditor),
            new FrameworkPropertyMetadata(default(Color),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnSelectedColorTextChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    public static void OnSelectedColorTextChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswTextEditor stsw)
        {
            stsw.FontColorText();
        }
    }

    /// <summary>
    /// Gets or sets the selected highlight color in the editor.
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
            typeof(StswTextEditor),
            new FrameworkPropertyMetadata(default(Color),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnSelectedColorHighlightChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    public static void OnSelectedColorHighlightChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswTextEditor stsw)
        {
            stsw.FontColorHighlight();
        }
    }

    /// <summary>
    /// Gets or sets the collection of sub controls to be displayed in the control.
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
            typeof(StswTextEditor)
        );

    /// <summary>
    /// Gets or sets a value indicating whether the control shows tool bar and how many options.
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
            typeof(StswTextEditor)
        );
    #endregion

    #region Style properties
    /// <summary>
    /// Gets or sets a value indicating whether corner clipping is enabled for the control.
    /// When set to <see langword="true"/>, content within the control's border area is clipped to match
    /// the border's rounded corners, preventing elements from protruding beyond the border.
    /// </summary>
    public bool CornerClipping
    {
        get => (bool)GetValue(CornerClippingProperty);
        set => SetValue(CornerClippingProperty, value);
    }
    public static readonly DependencyProperty CornerClippingProperty
        = DependencyProperty.Register(
            nameof(CornerClipping),
            typeof(bool),
            typeof(StswTextEditor)
        );

    /// <summary>
    /// Gets or sets the degree to which the corners of the control's border are rounded by defining
    /// a radius value for each corner independently. This property allows users to control the roundness
    /// of corners, and large radius values are smoothly scaled to blend from corner to corner.
    /// </summary>
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }
    public static readonly DependencyProperty CornerRadiusProperty
        = DependencyProperty.Register(
            nameof(CornerRadius),
            typeof(CornerRadius),
            typeof(StswTextEditor)
        );

    /// <summary>
    /// Gets or sets the thickness of the separator between box and subs.
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
            typeof(StswTextEditor)
        );

    /// <summary>
    /// Gets or sets the thickness of the buttons in the control.
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
            typeof(StswTextEditor)
        );

    /// <summary>
    /// Gets or sets the corner radius of the buttons in the control.
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
            typeof(StswTextEditor)
        );

    /// <summary>
    /// Gets or sets the margin of the buttons in the control.
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
            typeof(StswTextEditor)
        );
    #endregion
}
