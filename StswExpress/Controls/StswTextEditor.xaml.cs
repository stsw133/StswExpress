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
        SetValue(ComponentsProperty, new ObservableCollection<UIElement>());

        /// FILE
        FileNewCommand = new StswRelayCommand(FileNew_Executed, FileNew_CanExecute);
        FileOpenCommand = new StswRelayCommand(FileOpen_Executed, FileOpen_CanExecute);
        FileSaveCommand = new StswRelayCommand(FileSave_Executed, FileSave_CanExecute);
        FileSaveAsCommand = new StswRelayCommand(FileSaveAs_Executed, FileSaveAs_CanExecute);
        FileReloadCommand = new StswRelayCommand(FileReload_Executed, FileReload_CanExecute);
        FilePrintCommand = new StswRelayCommand(FilePrint_Executed, FilePrint_CanExecute);
        FileMailCommand = new StswRelayCommand(FileMail_Executed, FileMail_CanExecute);
        FileInfoCommand = new StswRelayCommand(FileInfo_Executed, FileInfo_CanExecute);
        /// EDIT
        EditUndoCommand = new StswRelayCommand(EditUndo_Executed, EditUndo_CanExecute);
        EditRedoCommand = new StswRelayCommand(EditRedo_Executed, EditRedo_CanExecute);
        EditClearCommand = new StswRelayCommand(EditClear_Executed, EditClear_CanExecute);
        EditSelectAllCommand = new StswRelayCommand(EditSelectAll_Executed, EditSelectAll_CanExecute);
        EditCutCommand = new StswRelayCommand(EditCut_Executed, EditCut_CanExecute);
        EditCopyCommand = new StswRelayCommand(EditCopy_Executed, EditCopy_CanExecute);
        EditPasteCommand = new StswRelayCommand(EditPaste_Executed, EditPaste_CanExecute);
        /// FONT
        FontBoldCommand = new StswRelayCommand(FontBold_Executed, FontBold_CanExecute);
        FontItalicCommand = new StswRelayCommand(FontItalic_Executed, FontItalic_CanExecute);
        FontUnderlineCommand = new StswRelayCommand(FontUnderline_Executed, FontUnderline_CanExecute);
        FontStrikethroughCommand = new StswRelayCommand(FontStrikethrough_Executed, FontStrikethrough_CanExecute);
        FontSubscriptCommand = new StswRelayCommand(FontSubscript_Executed, FontSubscript_CanExecute);
        FontSuperscriptCommand = new StswRelayCommand(FontSuperscript_Executed, FontSuperscript_CanExecute);
        FontColorTextCommand = new StswRelayCommand(FontColorText_Executed, FontColorText_CanExecute);
        FontColorHighlightCommand = new StswRelayCommand(FontColorHighlight_Executed, FontColorHighlight_CanExecute);
        /// SECTION
        SectionIncreaseCommand = new StswRelayCommand(SectionIncrease_Executed, SectionIncrease_CanExecute);
        SectionDecreaseCommand = new StswRelayCommand(SectionDecrease_Executed, SectionDecrease_CanExecute);
        SectionListBulletedCommand = new StswRelayCommand(SectionListBulleted_Executed, SectionListBulleted_CanExecute);
        SectionListNumberedCommand = new StswRelayCommand(SectionListNumbered_Executed, SectionListNumbered_CanExecute);
        SectionInterlineCommand = new StswRelayCommand<object?>(SectionInterline_Executed, SectionInterline_CanExecute);
        SectionAlignLeftCommand = new StswRelayCommand(SectionAlignLeft_Executed, SectionAlignLeft_CanExecute);
        SectionAlignCenterCommand = new StswRelayCommand(SectionAlignCenter_Executed, SectionAlignCenter_CanExecute);
        SectionAlignRightCommand = new StswRelayCommand(SectionAlignRight_Executed, SectionAlignRight_CanExecute);
        SectionAlignJustifyCommand = new StswRelayCommand(SectionAlignJustify_Executed, SectionAlignJustify_CanExecute);
    }
    static StswTextEditor()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswTextEditor), new FrameworkPropertyMetadata(typeof(StswTextEditor)));
    }

    #region Events
    private string _originalContent = string.Empty;
    private StswComboBox? partFontFamily;
    private StswNumericBox? partFontSize;

    /// OnApplyTemplate
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

    /// PART_Editor_SelectionChanged
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
    /// HasChanges
    private bool HasChanges() => new TextRange(Document.ContentStart, Document.ContentEnd).Text != _originalContent;

    /// ClearDocument
    private void ClearDocument()
    {
        Document.Blocks.Clear();
        _originalContent = string.Empty;

        IsUndoEnabled = !IsUndoEnabled;
        IsUndoEnabled = !IsUndoEnabled;
    }

    /// LoadDocument
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
    private void FileNew_Executed()
    {
        if (HasChanges() && MessageBox.Show("Czy na pewno chcesz porzucić zmiany i utworzyć nowy plik?", "Edytor tekstu", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
            return;

        FilePath = null;
        ClearDocument();
    }
    private bool FileNew_CanExecute() => true;

    /// Command: open
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
    private void FileInfo_Executed()
    {
        if (File.Exists(FilePath))
            Process.Start("explorer.exe", "/select, \"" + FilePath + "\"");
    }
    private bool FileInfo_CanExecute() => FilePath != null;

    /// Command: print
    private void FilePrint_Executed()
    {
        UpdateLayout();

        var printDialog = new PrintDialog();
        if (printDialog.ShowDialog() == true)
        {
            using (var stream = new MemoryStream())
            {
                var sourceDocument = new TextRange(Document.ContentStart, Document.ContentEnd);
                sourceDocument.Save(stream, DataFormats.Xaml);

                var flowDocumentCopy = new FlowDocument();
                var copyDocumentRange = new TextRange(flowDocumentCopy.ContentStart, flowDocumentCopy.ContentEnd);
                copyDocumentRange.Load(stream, DataFormats.Xaml);

                printDialog.PrintDocument(((IDocumentPaginatorSource)flowDocumentCopy).DocumentPaginator, "Printing Richtextbox Content");
            }
        }

        UpdateLayout();
    }
    private bool FilePrint_CanExecute() => true;

    /// Command: mail
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
    private void EditUndo_Executed()
    {
        Undo();
        Focus();
    }
    private bool EditUndo_CanExecute() => ApplicationCommands.Undo.CanExecute(null, this);

    /// Command: redo
    private void EditRedo_Executed()
    {
        Redo();
        Focus();
    }
    private bool EditRedo_CanExecute() => ApplicationCommands.Redo.CanExecute(null, this);

    /// Command: clear
    private void EditClear_Executed()
    {
        Selection.Text = string.Empty;
        Focus();
    }
    private bool EditClear_CanExecute() => EditingCommands.Delete.CanExecute(null, this);

    /// Command: select all
    private void EditSelectAll_Executed()
    {
        SelectAll();
        Focus();
    }
    private bool EditSelectAll_CanExecute() => ApplicationCommands.SelectAll.CanExecute(null, this);

    /// Command: cut
    private void EditCut_Executed()
    {
        Cut();
        Focus();
    }
    private bool EditCut_CanExecute() => ApplicationCommands.Cut.CanExecute(null, this);

    /// Command: copy
    private void EditCopy_Executed()
    {
        Copy();
        Focus();
    }
    private bool EditCopy_CanExecute() => ApplicationCommands.Copy.CanExecute(null, this);

    /// Command: paste
    private void EditPaste_Executed()
    {
        Paste();
        Focus();
    }
    private bool EditPaste_CanExecute() => ApplicationCommands.Paste.CanExecute(null, this);
    #endregion

    #region Font commands
    /// PART_FontFamily_SelectionChanged
    private void PART_FontFamily_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!Selection.IsEmpty && partFontFamily?.SelectedItem != null)
            Selection.ApplyPropertyValue(Inline.FontFamilyProperty, partFontFamily.SelectedItem);
        Focus();
    }

    /// PART_FontSize_TextChanged
    private void PART_FontSize_ValueChanged(object? sender, EventArgs e)
    {
        if (!Selection.IsEmpty && partFontSize?.Value != null)
            Selection.ApplyPropertyValue(Inline.FontSizeProperty, partFontSize.Value);
        Focus();
    }

    /// Command: bold
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
    private void FontSubscript_Executed()
    {
        EditingCommands.ToggleSubscript.Execute(null, this);
        Focus();
    }
    private bool FontSubscript_CanExecute() => EditingCommands.ToggleSubscript.CanExecute(null, this);

    /// Command: superscript
    private void FontSuperscript_Executed()
    {
        EditingCommands.ToggleSuperscript.Execute(null, this);
        Focus();
    }
    private bool FontSuperscript_CanExecute() => EditingCommands.ToggleSuperscript.CanExecute(null, this);

    /// Command: color
    private void FontColorText_Executed()
    {
        Selection.ApplyPropertyValue(Inline.ForegroundProperty, new SolidColorBrush(SelectedColorText));
        Focus();
    }
    private bool FontColorText_CanExecute() => true;

    /// Command: highlight
    private void FontColorHighlight_Executed()
    {
        Selection.ApplyPropertyValue(Inline.BackgroundProperty, new SolidColorBrush(SelectedColorHighlight));
        Focus();
    }
    private bool FontColorHighlight_CanExecute() => true;
    #endregion

    #region Section commands
    /// Command: increase
    private void SectionIncrease_Executed()
    {
        EditingCommands.IncreaseIndentation.Execute(null, this);
        Focus();
    }
    private bool SectionIncrease_CanExecute() => EditingCommands.IncreaseIndentation.CanExecute(null, this);

    /// Command: decrease
    private void SectionDecrease_Executed()
    {
        EditingCommands.DecreaseIndentation.Execute(null, this);
        Focus();
    }
    private bool SectionDecrease_CanExecute() => EditingCommands.DecreaseIndentation.CanExecute(null, this);

    /// Command: list bulleted
    private void SectionListBulleted_Executed()
    {
        EditingCommands.ToggleBullets.Execute(null, this);
        Focus();
    }
    private bool SectionListBulleted_CanExecute() => EditingCommands.ToggleBullets.CanExecute(null, this);

    /// Command: list numbered
    private void SectionListNumbered_Executed()
    {
        EditingCommands.ToggleNumbering.Execute(null, this);
        Focus();
    }
    private bool SectionListNumbered_CanExecute() => EditingCommands.ToggleNumbering.CanExecute(null, this);

    /// Command: interline
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
    private void SectionAlignLeft_Executed()
    {
        EditingCommands.AlignLeft.Execute(null, this);
        Focus();
    }
    private bool SectionAlignLeft_CanExecute() => EditingCommands.AlignLeft.CanExecute(null, this);

    /// Command: align center
    private void SectionAlignCenter_Executed()
    {
        EditingCommands.AlignCenter.Execute(null, this);
        Focus();
    }
    private bool SectionAlignCenter_CanExecute() => EditingCommands.AlignCenter.CanExecute(null, this);

    /// Command: align right
    private void SectionAlignRight_Executed()
    {
        EditingCommands.AlignRight.Execute(null, this);
        Focus();
    }
    private bool SectionAlignRight_CanExecute() => EditingCommands.AlignRight.CanExecute(null, this);

    /// Command: align justify
    private void SectionAlignJustify_Executed()
    {
        EditingCommands.AlignJustify.Execute(null, this);
        Focus();
    }
    private bool SectionAlignJustify_CanExecute() => EditingCommands.AlignJustify.CanExecute(null, this);
    #endregion

    #region Main properties
    /// Components
    public static readonly DependencyProperty ComponentsProperty
        = DependencyProperty.Register(
            nameof(Components),
            typeof(ObservableCollection<UIElement>),
            typeof(StswTextEditor)
        );
    public ObservableCollection<UIElement> Components
    {
        get => (ObservableCollection<UIElement>)GetValue(ComponentsProperty);
        set => SetValue(ComponentsProperty, value);
    }
    /// ComponentsAlignment
    public static readonly DependencyProperty ComponentsAlignmentProperty
        = DependencyProperty.Register(
            nameof(ComponentsAlignment),
            typeof(Dock),
            typeof(StswTextEditor)
        );
    public Dock ComponentsAlignment
    {
        get => (Dock)GetValue(ComponentsAlignmentProperty);
        set => SetValue(ComponentsAlignmentProperty, value);
    }

    /// FilePath
    public static readonly DependencyProperty FilePathProperty
        = DependencyProperty.Register(
            nameof(FilePath),
            typeof(string),
            typeof(StswTextEditor)
        );
    public string? FilePath
    {
        get => (string?)GetValue(FilePathProperty);
        set => SetValue(FilePathProperty, value);
    }

    /// IsExtended
    public static readonly DependencyProperty IsExtendedProperty
        = DependencyProperty.Register(
            nameof(IsExtended),
            typeof(bool),
            typeof(StswTextEditor)
        );
    public bool IsExtended
    {
        get => (bool)GetValue(IsExtendedProperty);
        set => SetValue(IsExtendedProperty, value);
    }

    /// SelectedColorText
    public static readonly DependencyProperty SelectedColorTextProperty
        = DependencyProperty.Register(
            nameof(SelectedColorText),
            typeof(Color),
            typeof(StswTextEditor),
            new FrameworkPropertyMetadata(default(Color),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnSelectedColorTextChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    public Color SelectedColorText
    {
        get => (Color)GetValue(SelectedColorTextProperty);
        internal set => SetValue(SelectedColorTextProperty, value);
    }
    public static void OnSelectedColorTextChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswTextEditor stsw)
            stsw.FontColorText_Executed();
    }
    /// SelectedColorHighlight
    public static readonly DependencyProperty SelectedColorHighlightProperty
        = DependencyProperty.Register(
            nameof(SelectedColorHighlight),
            typeof(Color),
            typeof(StswTextEditor),
            new FrameworkPropertyMetadata(default(Color),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnSelectedColorHighlightChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    public Color SelectedColorHighlight
    {
        get => (Color)GetValue(SelectedColorHighlightProperty);
        internal set => SetValue(SelectedColorHighlightProperty, value);
    }
    public static void OnSelectedColorHighlightChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswTextEditor stsw)
            stsw.FontColorHighlight_Executed();
    }
    #endregion

    #region Spatial properties
    /// > BorderThickness ...
    /// ComponentBorderThickness
    public static readonly DependencyProperty ComponentBorderThicknessProperty
        = DependencyProperty.Register(
            nameof(ComponentBorderThickness),
            typeof(Thickness),
            typeof(StswTextEditor)
        );
    public Thickness ComponentBorderThickness
    {
        get => (Thickness)GetValue(ComponentBorderThicknessProperty);
        set => SetValue(ComponentBorderThicknessProperty, value);
    }
    /// SubBorderThickness
    public static readonly DependencyProperty SubBorderThicknessProperty
        = DependencyProperty.Register(
            nameof(SubBorderThickness),
            typeof(Thickness),
            typeof(StswTextEditor)
        );
    public Thickness SubBorderThickness
    {
        get => (Thickness)GetValue(SubBorderThicknessProperty);
        set => SetValue(SubBorderThicknessProperty, value);
    }

    /// > CornerRadius ...
    /// CornerRadius
    public static readonly DependencyProperty CornerRadiusProperty
        = DependencyProperty.Register(
            nameof(CornerRadius),
            typeof(CornerRadius),
            typeof(StswTextEditor)
        );
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }
    /// SubCornerRadius
    public static readonly DependencyProperty SubCornerRadiusProperty
        = DependencyProperty.Register(
            nameof(SubCornerRadius),
            typeof(CornerRadius),
            typeof(StswTextEditor)
        );
    public CornerRadius SubCornerRadius
    {
        get => (CornerRadius)GetValue(SubCornerRadiusProperty);
        set => SetValue(SubCornerRadiusProperty, value);
    }

    /// > Padding ...
    /// SubPadding
    public static readonly DependencyProperty SubPaddingProperty
        = DependencyProperty.Register(
            nameof(SubPadding),
            typeof(Thickness),
            typeof(StswTextEditor)
        );
    public Thickness SubPadding
    {
        get => (Thickness)GetValue(SubPaddingProperty);
        set => SetValue(SubPaddingProperty, value);
    }
    #endregion
}
