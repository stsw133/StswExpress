using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace StswExpress;

/// <summary>
/// Interaction logic for ExtImage.xaml
/// </summary>
public partial class ExtImage : Border
{
    public ExtImage()
    {
        InitializeComponent();
    }

    /// ContextMenuVisibility
    public static readonly DependencyProperty ContextMenuVisibilityProperty
        = DependencyProperty.Register(
              nameof(ContextMenuVisibility),
              typeof(Visibility),
              typeof(ExtImage),
              new PropertyMetadata(default(Visibility))
          );
    public Visibility ContextMenuVisibility
    {
        get => (Visibility)GetValue(ContextMenuVisibilityProperty);
        set => SetValue(ContextMenuVisibilityProperty, value);
    }

    /// IsReadOnly
    public static readonly DependencyProperty IsReadOnlyProperty
        = DependencyProperty.Register(
              nameof(IsReadOnly),
              typeof(bool),
              typeof(ExtImage),
              new PropertyMetadata(default(bool))
          );
    public bool IsReadOnly
    {
        get => (bool)GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    /// Source
    public static readonly DependencyProperty SourceProperty
        = DependencyProperty.Register(
              nameof(Source),
              typeof(ImageSource),
              typeof(ExtImage),
              new PropertyMetadata(default(ImageSource?))
          );
    public ImageSource? Source
    {
        get => (ImageSource?)GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    /// Stretch
    public static readonly DependencyProperty StretchProperty
        = DependencyProperty.Register(
              nameof(Stretch),
              typeof(Stretch),
              typeof(ExtImage),
              new PropertyMetadata(default(Stretch))
          );
    public Stretch Stretch
    {
        get => (Stretch)GetValue(StretchProperty);
        set => SetValue(StretchProperty, value);
    }

    #region Events
    /// Cut
    private void MnuItmCut_Click(object sender, RoutedEventArgs e)
    {
        if (Source != null)
            Clipboard.SetImage(Source as BitmapSource);
        Source = null;
    }

    /// Copy
    private void MnuItmCopy_Click(object sender, RoutedEventArgs e)
    {
        if (Source != null)
            Clipboard.SetImage(Source as BitmapSource);
    }

    /// Paste
    private void MnuItmPaste_Click(object sender, RoutedEventArgs e)
    {
        if (Clipboard.GetImage().GetType() == typeof(InteropBitmap))
            Source = StswFn.ImageFromClipboard();
        else
            Source = Clipboard.GetImage();
    }

    /// Delete
    private void MnuItmDelete_Click(object sender, RoutedEventArgs e) => Source = null;

    /// Load
    private void MnuItmLoad_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog()
        {
            Filter = "All files (*.*)|*.*|BMP (*.bmp)|*.bmp|JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|GIF (*.gif)|*.gif|ICO (*.ico)|*.ico|PNG (*.png)|*.png"
        };
        try
        {
            if (dialog.ShowDialog() == true)
                Source = File.ReadAllBytes(dialog.FileName).AsImage();
        }
        catch { }
    }

    /// Save
    private void MnuItmSave_Click(object sender, RoutedEventArgs e)
    {
        if (Source == null)
            return;

        var dialog = new SaveFileDialog()
        {
            Filter = "PNG (*.png)|*.png|All files (*.*)|*.*"
        };
        if (dialog.ShowDialog() == true)
        {
            using var fileStream = new FileStream(dialog.FileName, FileMode.Create);
            BitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(Source as BitmapSource));
            encoder.Save(fileStream);
        }
    }
    #endregion
}
