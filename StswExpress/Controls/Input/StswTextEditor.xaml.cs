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
public class StswTextEditor : RichTextBox
{
    /// FILE
    public ICommand FileNewCommand { get; set; }
    public ICommand FileOpenCommand { get; set; }
    public ICommand FileSaveCommand { get; set; }
    public ICommand FileSaveAsCommand { get; set; }
    public ICommand FileReloadCommand { get; set; }
    public ICommand FilePrintCommand { get; set; }
    public ICommand FileMailCommand { get; set; }
    public ICommand FileInfoCommand { get; set; }
    /// EDIT
    public ICommand EditUndoCommand { get; set; }
    public ICommand EditRedoCommand { get; set; }
    public ICommand EditClearCommand { get; set; }
    public ICommand EditSelectAllCommand { get; set; }
    public ICommand EditCutCommand { get; set; }
    public ICommand EditCopyCommand { get; set; }
    public ICommand EditPasteCommand { get; set; }
    /// FONT
    public ICommand FontBoldCommand { get; set; }
    public ICommand FontItalicCommand { get; set; }
    public ICommand FontUnderlineCommand { get; set; }
    public ICommand FontStrikethroughCommand { get; set; }
    public ICommand FontSubscriptCommand { get; set; }
    public ICommand FontSuperscriptCommand { get; set; }
    public ICommand FontColorTextCommand { get; set; }
    public ICommand FontColorHighlightCommand { get; set; }
    /// SECTION
    public ICommand SectionIncreaseCommand { get; set; }
    public ICommand SectionDecreaseCommand { get; set; }
    public ICommand SectionListBulletedCommand { get; set; }
    public ICommand SectionListNumberedCommand { get; set; }
    public ICommand SectionInterlineCommand { get; set; }
    public ICommand SectionAlignLeftCommand { get; set; }
    public ICommand SectionAlignCenterCommand { get; set; }
    public ICommand SectionAlignRightCommand { get; set; }
    public ICommand SectionAlignJustifyCommand { get; set; }

    public StswTextEditor()
    {
        SetValue(ComponentsProperty, new ObservableCollection<IStswComponent>());

        /// FILE
        FileNewCommand = new StswCommand(FileNew_Executed, FileNew_CanExecute);
        FileOpenCommand = new StswCommand(FileOpen_Executed, FileOpen_CanExecute);
        FileSaveCommand = new StswCommand(FileSave_Executed, FileSave_CanExecute);
        FileSaveAsCommand = new StswCommand(FileSaveAs_Executed, FileSaveAs_CanExecute);
        FileReloadCommand = new StswCommand(FileReload_Executed, FileReload_CanExecute);
        FilePrintCommand = new StswCommand(FilePrint_Executed, FilePrint_CanExecute);
        FileMailCommand = new StswCommand(FileMail_Executed, FileMail_CanExecute);
        FileInfoCommand = new StswCommand(FileInfo_Executed, FileInfo_CanExecute);
        /// EDIT
        EditUndoCommand = new StswCommand(EditUndo_Executed, EditUndo_CanExecute);
        EditRedoCommand = new StswCommand(EditRedo_Executed, EditRedo_CanExecute);
        EditClearCommand = new StswCommand(EditClear_Executed, EditClear_CanExecute);
        EditSelectAllCommand = new StswCommand(EditSelectAll_Executed, EditSelectAll_CanExecute);
        EditCutCommand = new StswCommand(EditCut_Executed, EditCut_CanExecute);
        EditCopyCommand = new StswCommand(EditCopy_Executed, EditCopy_CanExecute);
        EditPasteCommand = new StswCommand(EditPaste_Executed, EditPaste_CanExecute);
        /// FONT
        FontBoldCommand = new StswCommand(FontBold_Executed, FontBold_CanExecute);
        FontItalicCommand = new StswCommand(FontItalic_Executed, FontItalic_CanExecute);
        FontUnderlineCommand = new StswCommand(FontUnderline_Executed, FontUnderline_CanExecute);
        FontStrikethroughCommand = new StswCommand(FontStrikethrough_Executed, FontStrikethrough_CanExecute);
        FontSubscriptCommand = new StswCommand(FontSubscript_Executed, FontSubscript_CanExecute);
        FontSuperscriptCommand = new StswCommand(FontSuperscript_Executed, FontSuperscript_CanExecute);
        FontColorTextCommand = new StswCommand(FontColorText_Executed, FontColorText_CanExecute);
        FontColorHighlightCommand = new StswCommand(FontColorHighlight_Executed, FontColorHighlight_CanExecute);
        /// SECTION
        SectionIncreaseCommand = new StswCommand(SectionIncrease_Executed, SectionIncrease_CanExecute);
        SectionDecreaseCommand = new StswCommand(SectionDecrease_Executed, SectionDecrease_CanExecute);
        SectionListBulletedCommand = new StswCommand(SectionListBulleted_Executed, SectionListBulleted_CanExecute);
        SectionListNumberedCommand = new StswCommand(SectionListNumbered_Executed, SectionListNumbered_CanExecute);
        SectionInterlineCommand = new StswCommand<object?>(SectionInterline_Executed, SectionInterline_CanExecute);
        SectionAlignLeftCommand = new StswCommand(SectionAlignLeft_Executed, SectionAlignLeft_CanExecute);
        SectionAlignCenterCommand = new StswCommand(SectionAlignCenter_Executed, SectionAlignCenter_CanExecute);
        SectionAlignRightCommand = new StswCommand(SectionAlignRight_Executed, SectionAlignRight_CanExecute);
        SectionAlignJustifyCommand = new StswCommand(SectionAlignJustify_Executed, SectionAlignJustify_CanExecute);
    }
    static StswTextEditor()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswTextEditor), new FrameworkPropertyMetadata(typeof(StswTextEditor)));
    }

    #region Events
    private string _originalContent = string.Empty;
    private StswComboBox? partFontFamily;
    private StswNumericBox? partFontSize;

    /// <summary>
    /// Occurs when the template is applied to the control.
    /// </summary>
    public override void OnApplyTemplate()
    {
        if (FilePath != null)
            LoadDocument();
        else
            Document.Blocks.Clear();

        //((Paragraph)Document.Blocks.FirstBlock).LineHeight = 0.0034;

        /// Box: font families
        if (GetTemplateChild("PART_FontFamily") is StswComboBox cmbFontFamily)
        {
            cmbFontFamily.ItemsSource = Fonts.SystemFontFamilies.OrderBy(x => x.Source);
            cmbFontFamily.SelectionChanged += PART_FontFamily_SelectionChanged;
            partFontFamily = cmbFontFamily;
        }
        /// Box: font size
        if (GetTemplateChild("PART_FontSize") is StswNumericBox nmbFontSize)
        {
            nmbFontSize.ValueChanged += PART_FontSize_ValueChanged;
            partFontSize = nmbFontSize;
        }

        SelectionChanged += PART_Editor_SelectionChanged;

        base.OnApplyTemplate();
    }

    /// <summary>
    /// Occurs when the editor's selection changes.
    /// </summary>
    private void PART_Editor_SelectionChanged(object sender, RoutedEventArgs e)
    {
        /*
        var temp = Selection.GetPropertyValue(Inline.FontWeightProperty);
        if (partFontBold != null)
            partFontBold.IsChecked = (temp != DependencyProperty.UnsetValue) && (temp.Equals(FontWeights.Bold));

        temp = Selection.GetPropertyValue(Inline.FontStyleProperty);
        if (partFontItalic != null)
            partFontItalic.IsChecked = (temp != DependencyProperty.UnsetValue) && (temp.Equals(FontStyles.Italic));

        temp = Selection.GetPropertyValue(Inline.TextDecorationsProperty);
        if (partFontUnderline != null)
            partFontUnderline.IsChecked = (temp != DependencyProperty.UnsetValue) && (temp.Equals(TextDecorations.Underline));
        */
        var temp = Selection.GetPropertyValue(Inline.FontFamilyProperty);
        if (partFontFamily != null)
            partFontFamily.SelectedItem = temp;

        temp = Selection.GetPropertyValue(Inline.FontSizeProperty);
        if (partFontSize != null)
        {
            if (temp != DependencyProperty.UnsetValue)
                partFontSize.Value = Convert.ToDouble(temp);
            else
                partFontSize.Value = null;
        }
    }
    #endregion

    #region File commands
    /// <summary>
    /// Checks whether there are any changes in the editor's content compared to the original content loaded from a file.
    /// </summary>
    private bool HasChanges() => new TextRange(Document.ContentStart, Document.ContentEnd).Text != _originalContent;

    /// <summary>
    /// Clears the content of the editor and resets the original content to an empty string.
    /// </summary>
    private void ClearDocument()
    {
        Document.Blocks.Clear();
        _originalContent = string.Empty;

        IsUndoEnabled = !IsUndoEnabled;
        IsUndoEnabled = !IsUndoEnabled;
    }

    /// <summary>
    /// Loads the content of the associated file path into the editor and sets it as the original content.
    /// If the file does not exist, the editor's content will be cleared, and the original content will be set to an empty string.
    /// </summary>
    private void LoadDocument()
    {
        if (File.Exists(FilePath))
        {
            using (var fileStream = new FileStream(FilePath, FileMode.Open))
            {
                var range = new TextRange(Document.ContentStart, Document.ContentEnd);
                range.Load(fileStream, DataFormats.Rtf);
                _originalContent = range.Text;
            }

            IsUndoEnabled = !IsUndoEnabled;
            IsUndoEnabled = !IsUndoEnabled;
        }
    }

    /// Command: new
    /// <summary>
    /// Creates a new empty document in the editor, discarding any unsaved changes if present.
    /// </summary>
    private void FileNew_Executed()
    {
        if (HasChanges() && MessageBox.Show("Czy na pewno chcesz porzucić zmiany i utworzyć nowy plik?", "Edytor tekstu", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
            return;

        FilePath = null;
        ClearDocument();
    }
    private bool FileNew_CanExecute() => true;

    /// Command: open
    /// <summary>
    /// Opens a file dialog to load a Rich Text Format (.rtf) file into the editor, discarding any unsaved changes if present.
    /// </summary>
    private void FileOpen_Executed()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Rich Text Format (*.rtf)|*.rtf|All files (*.*)|*.*"
        };
        if (dialog.ShowDialog() == true)
        {
            if (HasChanges() && MessageBox.Show("Czy na pewno chcesz porzucić zmiany i otworzyć wskazany plik?", "Edytor tekstu", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                return;

            FilePath = dialog.FileName;
            LoadDocument();
        }
    }
    private bool FileOpen_CanExecute() => true;

    /// Command: save
    /// <summary>
    /// Saves the content of the editor to the associated file path, or prompts the user to choose a file if the path is not set.
    /// </summary>
    private void FileSave_Executed()
    {
        if (!string.IsNullOrEmpty(FilePath))
            using (var fileStream = new FileStream(FilePath, FileMode.Create))
            {
                var range = new TextRange(Document.ContentStart, Document.ContentEnd);
                range.Save(fileStream, DataFormats.Rtf);
                _originalContent = range.Text;
            }
        else FileSaveAs_Executed();
    }
    private bool FileSave_CanExecute() => true;

    /// Command: save as
    /// <summary>
    /// Prompts the user to choose a file path and saves the content of the editor to the selected location in Rich Text Format (.rtf).
    /// </summary>
    private void FileSaveAs_Executed()
    {
        var dialog = new SaveFileDialog
        {
            Filter = "Rich Text Format (*.rtf)|*.rtf|All files (*.*)|*.*"
        };
        if (dialog.ShowDialog() == true)
        {
            using (var fileStream = new FileStream(dialog.FileName, FileMode.Create))
            {
                var range = new TextRange(Document.ContentStart, Document.ContentEnd);
                range.Save(fileStream, DataFormats.Rtf);
                _originalContent = range.Text;
            }
        }
    }
    private bool FileSaveAs_CanExecute() => true;

    /// Command: reload
    /// <summary>
    /// Reloads the content of the editor from the associated file path, discarding any unsaved changes if present.
    /// </summary>
    private void FileReload_Executed()
    {
        if (HasChanges() && MessageBox.Show("Czy na pewno chcesz porzucić zmiany i załadować ponownie plik?", "Edytor tekstu", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
            return;

        if (FilePath != null)
            LoadDocument();
        else
            ClearDocument();
    }
    private bool FileReload_CanExecute() => FilePath != null;

    /// Command: info
    /// <summary>
    /// Opens the file explorer with the currently associated file selected, if available.
    /// </summary>
    private void FileInfo_Executed()
    {
        if (File.Exists(FilePath))
            Process.Start("explorer.exe", "/select, \"" + FilePath + "\"");
    }
    private bool FileInfo_CanExecute() => FilePath != null;

    /// Command: print
    /// <summary>
    /// Prints the content of the editor using the system's default print dialog.
    /// </summary>
    private void FilePrint_Executed()
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
    private bool FilePrint_CanExecute() => true;

    /// Command: mail
    /// <summary>
    /// Opens the default email client with the editor's content as the email body for sharing.
    /// </summary>
    private void FileMail_Executed()
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
    private bool FileMail_CanExecute() => true;
    #endregion

    #region Edit commands
    /// Command: undo
    /// <summary>
    /// Undoes the most recent action in the editor.
    /// </summary>
    private void EditUndo_Executed()
    {
        Undo();
        Focus();
    }
    private bool EditUndo_CanExecute() => ApplicationCommands.Undo.CanExecute(null, this);

    /// Command: redo
    /// <summary>
    /// Redoes the most recently undone action in the editor.
    /// </summary>
    private void EditRedo_Executed()
    {
        Redo();
        Focus();
    }
    private bool EditRedo_CanExecute() => ApplicationCommands.Redo.CanExecute(null, this);

    /// Command: clear
    /// <summary>
    /// Clears the selected text in the editor.
    /// </summary>
    private void EditClear_Executed()
    {
        Selection.Text = string.Empty;
        Focus();
    }
    private bool EditClear_CanExecute() => EditingCommands.Delete.CanExecute(null, this);

    /// Command: select all
    /// <summary>
    /// Selects all text in the editor.
    /// </summary>
    private void EditSelectAll_Executed()
    {
        SelectAll();
        Focus();
    }
    private bool EditSelectAll_CanExecute() => ApplicationCommands.SelectAll.CanExecute(null, this);

    /// Command: cut
    /// <summary>
    /// Cuts the selected text from the editor and copies it to the clipboard.
    /// </summary>
    private void EditCut_Executed()
    {
        Cut();
        Focus();
    }
    private bool EditCut_CanExecute() => ApplicationCommands.Cut.CanExecute(null, this);

    /// Command: copy
    /// <summary>
    /// Copies the selected text to the clipboard.
    /// </summary>
    private void EditCopy_Executed()
    {
        Copy();
        Focus();
    }
    private bool EditCopy_CanExecute() => ApplicationCommands.Copy.CanExecute(null, this);

    /// Command: paste
    /// <summary>
    /// Pastes the contents of the clipboard into the editor.
    /// </summary>
    private void EditPaste_Executed()
    {
        Paste();
        Focus();
    }
    private bool EditPaste_CanExecute() => ApplicationCommands.Paste.CanExecute(null, this);
    #endregion

    #region Font commands
    /// <summary>
    /// Event handler for the selection changed event of the font family combo box.
    /// Applies the selected font family to the text of the selected portion in the editor.
    /// </summary>
    private void PART_FontFamily_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!Selection.IsEmpty && partFontFamily?.SelectedItem != null)
            Selection.ApplyPropertyValue(Inline.FontFamilyProperty, partFontFamily.SelectedItem);
        Focus();
    }

    /// <summary>
    /// Event handler for the value changed event of the font size numeric box.
    /// Applies the selected font size to the text of the selected portion in the editor.
    /// </summary>
    private void PART_FontSize_ValueChanged(object? sender, EventArgs e)
    {
        if (!Selection.IsEmpty && partFontSize?.Value != null)
            Selection.ApplyPropertyValue(Inline.FontSizeProperty, partFontSize.Value);
        Focus();
    }

    /// Command: bold
    /// <summary>
    /// Toggles the bold formatting for the selected text.
    /// </summary>
    private void FontBold_Executed()
    {
        var newFontWeight = FontWeights.Bold;
        if (Selection.GetPropertyValue(Inline.FontWeightProperty) is FontWeight current && current == FontWeights.Bold)
            newFontWeight = new FontWeight();

        Selection.ApplyPropertyValue(Inline.FontWeightProperty, newFontWeight);
        Focus();
    }
    private bool FontBold_CanExecute() => EditingCommands.ToggleBold.CanExecute(null, this);

    /// Command: italic
    /// <summary>
    /// Toggles the italic formatting for the selected text.
    /// </summary>
    private void FontItalic_Executed()
    {
        var newFontStyle = FontStyles.Italic;
        if (Selection.GetPropertyValue(Inline.FontStyleProperty) is FontStyle current && current == FontStyles.Italic)
            newFontStyle = new FontStyle();

        Selection.ApplyPropertyValue(Inline.FontStyleProperty, newFontStyle);
        Focus();
    }
    private bool FontItalic_CanExecute() => EditingCommands.ToggleItalic.CanExecute(null, this);

    /// Command: underline
    /// <summary>
    /// Toggles the underline formatting for the selected text.
    /// </summary>
    private void FontUnderline_Executed()
    {
        var newTextDecoration = TextDecorations.Underline;
        if (Selection.GetPropertyValue(Inline.TextDecorationsProperty) is TextDecorationCollection current && current == TextDecorations.Underline)
            newTextDecoration = new TextDecorationCollection();

        Selection.ApplyPropertyValue(Inline.TextDecorationsProperty, newTextDecoration);
        Focus();
    }
    private bool FontUnderline_CanExecute() => EditingCommands.ToggleUnderline.CanExecute(null, this);

    /// Command: strikethrough
    /// <summary>
    /// Toggles the strikethrough formatting for the selected text.
    /// </summary>
    private void FontStrikethrough_Executed()
    {
        var newTextDecoration = TextDecorations.Strikethrough;
        if (Selection.GetPropertyValue(Inline.TextDecorationsProperty) is TextDecorationCollection current && current == TextDecorations.Strikethrough)
            newTextDecoration = new TextDecorationCollection();

        Selection.ApplyPropertyValue(Inline.TextDecorationsProperty, newTextDecoration);
        Focus();
    }
    private bool FontStrikethrough_CanExecute() => EditingCommands.ToggleUnderline.CanExecute(null, this);

    /// Command: subscript
    /// <summary>
    /// Toggles the subscript formatting for the selected text.
    /// </summary>
    private void FontSubscript_Executed()
    {
        EditingCommands.ToggleSubscript.Execute(null, this);
        Focus();
    }
    private bool FontSubscript_CanExecute() => EditingCommands.ToggleSubscript.CanExecute(null, this);

    /// Command: superscript
    /// <summary>
    /// Toggles the superscript formatting for the selected text.
    /// </summary>
    private void FontSuperscript_Executed()
    {
        EditingCommands.ToggleSuperscript.Execute(null, this);
        Focus();
    }
    private bool FontSuperscript_CanExecute() => EditingCommands.ToggleSuperscript.CanExecute(null, this);

    /// Command: color
    /// <summary>
    /// Applies the selected color to the text of the selected portion in the editor.
    /// </summary>
    private void FontColorText_Executed()
    {
        Selection.ApplyPropertyValue(Inline.ForegroundProperty, new SolidColorBrush(SelectedColorText));
        Focus();
    }
    private bool FontColorText_CanExecute() => true;

    /// Command: highlight
    /// <summary>
    /// Applies the selected color as a highlight to the selected portion in the editor.
    /// </summary>
    private void FontColorHighlight_Executed()
    {
        Selection.ApplyPropertyValue(Inline.BackgroundProperty, new SolidColorBrush(SelectedColorHighlight));
        Focus();
    }
    private bool FontColorHighlight_CanExecute() => true;
    #endregion

    #region Section commands
    /// Command: increase
    /// <summary>
    /// Increases the indentation level of the selected section in the editor.
    /// </summary>
    private void SectionIncrease_Executed()
    {
        EditingCommands.IncreaseIndentation.Execute(null, this);
        Focus();
    }
    private bool SectionIncrease_CanExecute() => EditingCommands.IncreaseIndentation.CanExecute(null, this);

    /// Command: decrease
    /// <summary>
    /// Decreases the indentation level of the selected section in the editor.
    /// </summary>
    private void SectionDecrease_Executed()
    {
        EditingCommands.DecreaseIndentation.Execute(null, this);
        Focus();
    }
    private bool SectionDecrease_CanExecute() => EditingCommands.DecreaseIndentation.CanExecute(null, this);

    /// Command: list bulleted
    /// <summary>
    /// Toggles bulleted list formatting for the selected text.
    /// </summary>
    private void SectionListBulleted_Executed()
    {
        EditingCommands.ToggleBullets.Execute(null, this);
        Focus();
    }
    private bool SectionListBulleted_CanExecute() => EditingCommands.ToggleBullets.CanExecute(null, this);

    /// Command: list numbered
    /// <summary>
    /// Toggles numbered list formatting for the selected text.
    /// </summary>
    private void SectionListNumbered_Executed()
    {
        EditingCommands.ToggleNumbering.Execute(null, this);
        Focus();
    }
    private bool SectionListNumbered_CanExecute() => EditingCommands.ToggleNumbering.CanExecute(null, this);

    /// Command: interline
    /// <summary>
    /// Adjusts the interline spacing for the selected paragraph(s) in the editor.
    /// </summary>
    private void SectionInterline_Executed(object? parameter)
    {
        parameter ??= double.NaN;

        var curCaret = CaretPosition;
        var curBlocks = Document.Blocks.Where(block => block.ContentEnd.CompareTo(Selection.Start) > 0 && block.ContentStart.CompareTo(Selection.End) < 0);
        foreach (var curBlock in curBlocks)
            curBlock.Margin = new Thickness(0, 0, 0, Convert.ToDouble(parameter));

        Focus();
    }
    private bool SectionInterline_CanExecute() => true;

    /// Command: align left
    /// <summary>
    /// Aligns the selected text to the left in the editor.
    /// </summary>
    private void SectionAlignLeft_Executed()
    {
        EditingCommands.AlignLeft.Execute(null, this);
        Focus();
    }
    private bool SectionAlignLeft_CanExecute() => EditingCommands.AlignLeft.CanExecute(null, this);

    /// Command: align center
    /// <summary>
    /// Aligns the selected text to the center in the editor.
    /// </summary>
    private void SectionAlignCenter_Executed()
    {
        EditingCommands.AlignCenter.Execute(null, this);
        Focus();
    }
    private bool SectionAlignCenter_CanExecute() => EditingCommands.AlignCenter.CanExecute(null, this);

    /// Command: align right
    /// <summary>
    /// Aligns the selected text to the right in the editor.
    /// </summary>
    private void SectionAlignRight_Executed()
    {
        EditingCommands.AlignRight.Execute(null, this);
        Focus();
    }
    private bool SectionAlignRight_CanExecute() => EditingCommands.AlignRight.CanExecute(null, this);

    /// Command: align justify
    /// <summary>
    /// Justifies the selected text in the editor.
    /// </summary>
    private void SectionAlignJustify_Executed()
    {
        EditingCommands.AlignJustify.Execute(null, this);
        Focus();
    }
    private bool SectionAlignJustify_CanExecute() => EditingCommands.AlignJustify.CanExecute(null, this);
    #endregion

    #region Main properties
    /// <summary>
    /// Gets or sets the collection of components to be displayed in the control.
    /// </summary>
    public ObservableCollection<IStswComponent> Components
    {
        get => (ObservableCollection<IStswComponent>)GetValue(ComponentsProperty);
        set => SetValue(ComponentsProperty, value);
    }
    public static readonly DependencyProperty ComponentsProperty
        = DependencyProperty.Register(
            nameof(Components),
            typeof(ObservableCollection<IStswComponent>),
            typeof(StswTextEditor)
        );

    /// <summary>
    /// Gets or sets the alignment of the components within the control.
    /// </summary>
    public Dock ComponentsAlignment
    {
        get => (Dock)GetValue(ComponentsAlignmentProperty);
        set => SetValue(ComponentsAlignmentProperty, value);
    }
    public static readonly DependencyProperty ComponentsAlignmentProperty
        = DependencyProperty.Register(
            nameof(ComponentsAlignment),
            typeof(Dock),
            typeof(StswTextEditor)
        );

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
            typeof(StswTextEditor)
        );

    /// <summary>
    /// Gets or sets a value indicating whether the control is in extended mode (shows more options in component panel).
    /// </summary>
    public bool IsExtended
    {
        get => (bool)GetValue(IsExtendedProperty);
        set => SetValue(IsExtendedProperty, value);
    }
    public static readonly DependencyProperty IsExtendedProperty
        = DependencyProperty.Register(
            nameof(IsExtended),
            typeof(bool),
            typeof(StswTextEditor)
        );

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
            stsw.FontColorText_Executed();
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
            stsw.FontColorHighlight_Executed();
    }
    #endregion

    #region Style properties
    /// <summary>
    /// Gets or sets the thickness of the buttons in the control.
    /// </summary>
    public Thickness ComponentBorderThickness
    {
        get => (Thickness)GetValue(ComponentBorderThicknessProperty);
        set => SetValue(ComponentBorderThicknessProperty, value);
    }
    public static readonly DependencyProperty ComponentBorderThicknessProperty
        = DependencyProperty.Register(
            nameof(ComponentBorderThickness),
            typeof(Thickness),
            typeof(StswTextEditor)
        );

    /// <summary>
    /// Gets or sets the degree to which the corners of the control are rounded.
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
    /// Gets or sets the thickness of the border used as separator between box and components.
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
