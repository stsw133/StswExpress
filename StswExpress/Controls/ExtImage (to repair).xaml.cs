using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace StswExpress;

/// <summary>
/// Interaction logic for ExtImage.xaml
/// </summary>
public partial class ExtImage : Image
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

    /// Cut
    private void MnuItmCut_Click(object sender, RoutedEventArgs e)
    {
        Clipboard.SetImage(Source as BitmapSource);
        Source = new BitmapImage(new Uri($"pack://application:,,,/StswExpress;component/Resources/empty.png"));
    }

    /// Copy
    private void MnuItmCopy_Click(object sender, RoutedEventArgs e) => Clipboard.SetImage(Source as BitmapSource);

    /// Paste
    private void MnuItmPaste_Click(object sender, RoutedEventArgs e) => Source = Clipboard.GetImage();

    /// Delete
    private void MnuItmDelete_Click(object sender, RoutedEventArgs e) => Source = new BitmapImage(new Uri($"pack://application:,,,/StswExpress;component/Resources/empty.png"));

    /// Load
    private void MnuItmLoad_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog()
        {
            Filter = "All files (*.*)|*.*|BMP (*.bmp)|*.bmp|JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|GIF (*.gif)|*.gif|ICO (*.ico)|*.ico|PNG (*.png)|*.png"
        };
        if (dialog.ShowDialog() == true)
            Source = File.ReadAllBytes(dialog.FileName).AsImage();
    }

    /// Save
    private void MnuItmSave_Click(object sender, RoutedEventArgs e)
    {
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
}
