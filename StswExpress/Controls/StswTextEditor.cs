using Microsoft.Win32;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace StswExpress;

public class StswTextEditor : RichTextBox
{
    public StswTextEditor()
    {
        SetValue(ButtonsProperty, new ObservableCollection<UIElement>());
    }
    static StswTextEditor()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswTextEditor), new FrameworkPropertyMetadata(typeof(StswTextEditor)));
    }

    #region Events
    private StswComboBox? cmbFontFamily, cmbFontSize;

    /// OnApplyTemplate
    public override void OnApplyTemplate()
    {
        ((Paragraph)Document.Blocks.FirstBlock).LineHeight = 0.0034;

        /// Button: load
        if (GetTemplateChild("PART_ButtonLoad") is Button btnLoad)
            btnLoad.Click += PART_ButtonLoad_Click;
        /// Button: save
        if (GetTemplateChild("PART_ButtonSave") is Button btnSave)
            btnSave.Click += PART_ButtonSave_Click;
        /*
        /// Button: format bold
        if (GetTemplateChild("PART_ButtonFormatBold") is StswToggleButton btnFormatBold)
        {
            btnFormatBold.Click += PART_ButtonFormatBold_Click;
            this.btnFormatBold = btnFormatBold;
        }
        /// Button: format italic
        if (GetTemplateChild("PART_ButtonFormatItalic") is StswToggleButton btnFormatItalic)
        {
            btnFormatItalic.Click += PART_ButtonFormatItalic_Click;
            this.btnFormatItalic = btnFormatItalic;
        }
        /// Button: format underline
        if (GetTemplateChild("PART_ButtonFormatUnderline") is StswToggleButton btnFormatUnderline)
        {
            btnFormatUnderline.Click += PART_ButtonFormatItalic_Click;
            this.btnFormatUnderline = btnFormatUnderline;
        }
        */
        /// Box: font families
        if (GetTemplateChild("PART_BoxFontFamily") is StswComboBox cmbFontFamily)
        {
            cmbFontFamily.ItemsSource = Fonts.SystemFontFamilies.OrderBy(x => x.Source);
            cmbFontFamily.SelectionChanged += PART_BoxFontFamily_SelectionChanged;
            this.cmbFontFamily = cmbFontFamily;
        }
        /// Box: font size
        if (GetTemplateChild("PART_BoxFontSize") is StswComboBox cmbFontSize)
        {
            cmbFontSize.ItemsSource = new List<double>() { 8, 9, 10, 11, 12, 14, 16, 18, 20, 22, 24, 26, 28, 36, 48, 72 };
            cmbFontSize.SelectionChanged += PART_BoxFontSize_SelectionChanged;
            this.cmbFontSize = cmbFontSize;
        }

        SelectionChanged += PART_Editor_SelectionChanged;

        base.OnApplyTemplate();
    }

    /// PART_Editor_SelectionChanged
    private void PART_Editor_SelectionChanged(object sender, RoutedEventArgs e)
    {
        /*
        object temp = Selection.GetPropertyValue(Inline.FontWeightProperty);
        if (btnFormatBold != null)
            btnFormatBold.IsChecked = (temp != DependencyProperty.UnsetValue) && (temp.Equals(FontWeights.Bold));

        temp = Selection.GetPropertyValue(Inline.FontStyleProperty);
        if (btnFormatItalic != null)
            btnFormatItalic.IsChecked = (temp != DependencyProperty.UnsetValue) && (temp.Equals(FontStyles.Italic));

        temp = Selection.GetPropertyValue(Inline.TextDecorationsProperty);
        if (btnFormatUnderline != null)
            btnFormatUnderline.IsChecked = (temp != DependencyProperty.UnsetValue) && (temp.Equals(TextDecorations.Underline));
        */
        var temp = Selection.GetPropertyValue(Inline.FontFamilyProperty);
        if (cmbFontFamily != null)
            cmbFontFamily.SelectedItem = temp;

        temp = Selection.GetPropertyValue(Inline.FontSizeProperty);
        if (cmbFontSize != null)
            cmbFontSize.Text = temp.ToString();
    }

    /// PART_ButtonLoad_Click
    private void PART_ButtonLoad_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Rich Text Format (*.rtf)|*.rtf|All files (*.*)|*.*"
        };
        if (dialog.ShowDialog() == true)
        {
            var fileStream = new FileStream(dialog.FileName, FileMode.Open);
            var range = new TextRange(Document.ContentStart, Document.ContentEnd);
            range.Load(fileStream, DataFormats.Rtf);
        }
    }

    /// PART_ButtonSave_Click
    private void PART_ButtonSave_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new SaveFileDialog
        {
            Filter = "Rich Text Format (*.rtf)|*.rtf|All files (*.*)|*.*"
        };
        if (dialog.ShowDialog() == true)
        {
            var fileStream = new FileStream(dialog.FileName, FileMode.Create);
            var range = new TextRange(Document.ContentStart, Document.ContentEnd);
            range.Save(fileStream, DataFormats.Rtf);
        }
    }

    /// PART_FontFamilies_SelectionChanged
    private void PART_BoxFontFamily_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (cmbFontFamily?.SelectedItem != null)
            Selection.ApplyPropertyValue(Inline.FontFamilyProperty, cmbFontFamily.SelectedItem);
    }

    /// PART_FontSizes_SelectionChanged
    private void PART_BoxFontSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (cmbFontSize?.SelectedItem != null)
            Selection.ApplyPropertyValue(Inline.FontSizeProperty, cmbFontSize.SelectedItem);
    }
    #endregion

    #region Properties
    /// Buttons
    public static readonly DependencyProperty ButtonsProperty
        = DependencyProperty.Register(
            nameof(Buttons),
            typeof(ObservableCollection<UIElement>),
            typeof(StswTextEditor)
        );
    public ObservableCollection<UIElement> Buttons
    {
        get => (ObservableCollection<UIElement>)GetValue(ButtonsProperty);
        set => SetValue(ButtonsProperty, value);
    }
    /// ButtonsAlignment
    public static readonly DependencyProperty ButtonsAlignmentProperty
        = DependencyProperty.Register(
            nameof(ButtonsAlignment),
            typeof(Dock),
            typeof(StswTextEditor)
        );
    public Dock ButtonsAlignment
    {
        get => (Dock)GetValue(ButtonsAlignmentProperty);
        set => SetValue(ButtonsAlignmentProperty, value);
    }

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
    #endregion

    #region Style
    /// > Background ...
    /// BackgroundDisabled
    public static readonly DependencyProperty BackgroundDisabledProperty
        = DependencyProperty.Register(
            nameof(BackgroundDisabled),
            typeof(Brush),
            typeof(StswTextEditor)
        );
    public Brush BackgroundDisabled
    {
        get => (Brush)GetValue(BackgroundDisabledProperty);
        set => SetValue(BackgroundDisabledProperty, value);
    }
    /// BackgroundMouseOver
    public static readonly DependencyProperty BackgroundMouseOverProperty
        = DependencyProperty.Register(
            nameof(BackgroundMouseOver),
            typeof(Brush),
            typeof(StswTextEditor)
        );
    public Brush BackgroundMouseOver
    {
        get => (Brush)GetValue(BackgroundMouseOverProperty);
        set => SetValue(BackgroundMouseOverProperty, value);
    }
    /// BackgroundFocused
    public static readonly DependencyProperty BackgroundFocusedProperty
        = DependencyProperty.Register(
            nameof(BackgroundFocused),
            typeof(Brush),
            typeof(StswTextEditor)
        );
    public Brush BackgroundFocused
    {
        get => (Brush)GetValue(BackgroundFocusedProperty);
        set => SetValue(BackgroundFocusedProperty, value);
    }
    /// BackgroundReadOnly
    public static readonly DependencyProperty BackgroundReadOnlyProperty
        = DependencyProperty.Register(
            nameof(BackgroundReadOnly),
            typeof(Brush),
            typeof(StswTextEditor)
        );
    public Brush BackgroundReadOnly
    {
        get => (Brush)GetValue(BackgroundReadOnlyProperty);
        set => SetValue(BackgroundReadOnlyProperty, value);
    }

    /// > BorderBrush ...
    /// BorderBrushDisabled
    public static readonly DependencyProperty BorderBrushDisabledProperty
        = DependencyProperty.Register(
            nameof(BorderBrushDisabled),
            typeof(Brush),
            typeof(StswTextEditor)
        );
    public Brush BorderBrushDisabled
    {
        get => (Brush)GetValue(BorderBrushDisabledProperty);
        set => SetValue(BorderBrushDisabledProperty, value);
    }
    /// BorderBrushMouseOver
    public static readonly DependencyProperty BorderBrushMouseOverProperty
        = DependencyProperty.Register(
            nameof(BorderBrushMouseOver),
            typeof(Brush),
            typeof(StswTextEditor)
        );
    public Brush BorderBrushMouseOver
    {
        get => (Brush)GetValue(BorderBrushMouseOverProperty);
        set => SetValue(BorderBrushMouseOverProperty, value);
    }
    /// BorderBrushFocused
    public static readonly DependencyProperty BorderBrushFocusedProperty
        = DependencyProperty.Register(
            nameof(BorderBrushFocused),
            typeof(Brush),
            typeof(StswTextEditor)
        );
    public Brush BorderBrushFocused
    {
        get => (Brush)GetValue(BorderBrushFocusedProperty);
        set => SetValue(BorderBrushFocusedProperty, value);
    }

    /// > Foreground ...
    /// ForegroundDisabled
    public static readonly DependencyProperty ForegroundDisabledProperty
        = DependencyProperty.Register(
            nameof(ForegroundDisabled),
            typeof(Brush),
            typeof(StswTextEditor)
        );
    public Brush ForegroundDisabled
    {
        get => (Brush)GetValue(ForegroundDisabledProperty);
        set => SetValue(ForegroundDisabledProperty, value);
    }
    /// ForegroundMouseOver
    public static readonly DependencyProperty ForegroundMouseOverProperty
        = DependencyProperty.Register(
            nameof(ForegroundMouseOver),
            typeof(Brush),
            typeof(StswTextEditor)
        );
    public Brush ForegroundMouseOver
    {
        get => (Brush)GetValue(ForegroundMouseOverProperty);
        set => SetValue(ForegroundMouseOverProperty, value);
    }
    /// ForegroundFocused
    public static readonly DependencyProperty ForegroundFocusedProperty
        = DependencyProperty.Register(
            nameof(ForegroundFocused),
            typeof(Brush),
            typeof(StswTextEditor)
        );
    public Brush ForegroundFocused
    {
        get => (Brush)GetValue(ForegroundFocusedProperty);
        set => SetValue(ForegroundFocusedProperty, value);
    }

    /// > BorderThickness ...
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
    #endregion
}
