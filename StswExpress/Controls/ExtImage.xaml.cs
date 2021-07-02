using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace StswExpress
{
    /// <summary>
    /// Interaction logic for ExtImage.xaml
    /// </summary>
    public partial class ExtImage : Image
    {
        public ExtImage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// IsContextMenuVisible
        /// </summary>
        public static readonly DependencyProperty IsContextMenuVisibleProperty
            = DependencyProperty.Register(
                  nameof(IsContextMenuVisible),
                  typeof(bool),
                  typeof(ExtImage),
                  new PropertyMetadata(false)
              );
        public bool IsContextMenuVisible
        {
            get => (bool)GetValue(IsContextMenuVisibleProperty);
            set
            {
                ContextMenu.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
                SetValue(IsContextMenuVisibleProperty, value);
            }
        }

        /// <summary>
        /// IsReadOnly
        /// </summary>
        public static readonly DependencyProperty IsReadOnlyProperty
            = DependencyProperty.Register(
                  nameof(IsReadOnly),
                  typeof(bool),
                  typeof(ExtImage),
                  new PropertyMetadata(false)
              );
        public bool IsReadOnly
        {
            get => (bool)GetValue(IsReadOnlyProperty);
            set
            {
                var items = ContextMenu.Items;

                (items[0] as MenuItem).IsEnabled = !value;
                (items[2] as MenuItem).IsEnabled = !value;
                (items[3] as MenuItem).IsEnabled = !value;
                (items[5] as MenuItem).IsEnabled = !value;

                ((items[0] as MenuItem).Icon as TextBlock).Foreground = value ? Brushes.Gray : (SolidColorBrush)new BrushConverter().ConvertFrom("#58B");
                ((items[1] as MenuItem).Icon as TextBlock).Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#58B");
                ((items[2] as MenuItem).Icon as TextBlock).Foreground = value ? Brushes.Gray : (SolidColorBrush)new BrushConverter().ConvertFrom("#58B");
                ((items[3] as MenuItem).Icon as TextBlock).Foreground = value ? Brushes.Gray : (SolidColorBrush)new BrushConverter().ConvertFrom("#A22");
                ((items[5] as MenuItem).Icon as TextBlock).Foreground = value ? Brushes.Gray : (SolidColorBrush)new BrushConverter().ConvertFrom("#C84");
                ((items[6] as MenuItem).Icon as TextBlock).Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#84C");

                SetValue(IsReadOnlyProperty, value);
            }
        }

        /// <summary>
        /// Loaded
        /// </summary>
        private void Image_Loaded(object sender, RoutedEventArgs e)
        {
            IsContextMenuVisible = IsContextMenuVisible;
            IsReadOnly = IsReadOnly;
        }

        /// <summary>
        /// Cut
        /// </summary>
        private void MnuItmCut_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetImage(Source as BitmapSource);
            Source = null;
        }

        /// <summary>
        /// Copy
        /// </summary>
        private void MnuItmCopy_Click(object sender, RoutedEventArgs e) => Clipboard.SetImage(Source as BitmapSource);

        /// <summary>
        /// Paste
        /// </summary>
        private void MnuItmPaste_Click(object sender, RoutedEventArgs e) => Source = Clipboard.GetImage();

        /// <summary>
        /// Delete
        /// </summary>
        private void MnuItmDelete_Click(object sender, RoutedEventArgs e) => Source = null;

        /// <summary>
        /// Load
        /// </summary>
        private void MnuItmLoad_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog()
            {
                Filter = "All files (*.*)|*.*|BMP (*.bmp)|*.bmp|JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|GIF (*.gif)|*.gif|ICO (*.ico)|*.ico|PNG (*.png)|*.png"
            };
            if (dialog.ShowDialog() == true)
                Source = Fn.LoadImage(File.ReadAllBytes(dialog.FileName));
        }

        /// <summary>
        /// Save
        /// </summary>
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
}
