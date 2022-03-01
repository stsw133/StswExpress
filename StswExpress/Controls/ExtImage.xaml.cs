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
        /// ContextMenuVisibility
        /// </summary>
        public static readonly DependencyProperty ContextMenuVisibilityProperty
            = DependencyProperty.Register(
                  nameof(ContextMenuVisibility),
                  typeof(Visibility),
                  typeof(ExtImage),
                  new PropertyMetadata(Visibility.Collapsed)
              );
        public Visibility ContextMenuVisibility
        {
            get => (Visibility)GetValue(ContextMenuVisibilityProperty);
            set => SetValue(ContextMenuVisibilityProperty, value);
        }

        /// <summary>
        /// IsReadOnly
        /// </summary>
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
            set
            {
                var items = ContextMenu.Items;

                ((MenuItem)items[0]).IsEnabled = !value;
                ((MenuItem)items[2]).IsEnabled = !value;
                ((MenuItem)items[3]).IsEnabled = !value;
                ((MenuItem)items[5]).IsEnabled = !value;

                ((TextBlock)((MenuItem)items[0]).Icon).Foreground = value ? Brushes.Gray : (SolidColorBrush)new BrushConverter().ConvertFrom("#58B");
                ((TextBlock)((MenuItem)items[1]).Icon).Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#58B");
                ((TextBlock)((MenuItem)items[2]).Icon).Foreground = value ? Brushes.Gray : (SolidColorBrush)new BrushConverter().ConvertFrom("#58B");
                ((TextBlock)((MenuItem)items[3]).Icon).Foreground = value ? Brushes.Gray : (SolidColorBrush)new BrushConverter().ConvertFrom("#A22");
                ((TextBlock)((MenuItem)items[5]).Icon).Foreground = value ? Brushes.Gray : (SolidColorBrush)new BrushConverter().ConvertFrom("#C84");
                ((TextBlock)((MenuItem)items[6]).Icon).Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#84C");

                SetValue(IsReadOnlyProperty, value);
            }
        }

        /// Loaded
        private void Image_Loaded(object sender, RoutedEventArgs e)
        {
            ContextMenuVisibility = ContextMenuVisibility;
            IsReadOnly = IsReadOnly;
        }

        /// Cut
        private void MnuItmCut_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetImage(Source as BitmapSource);
            Source = null;
        }

        /// Copy
        private void MnuItmCopy_Click(object sender, RoutedEventArgs e) => Clipboard.SetImage(Source as BitmapSource);

        /// Paste
        private void MnuItmPaste_Click(object sender, RoutedEventArgs e) => Source = Clipboard.GetImage();

        /// Delete
        private void MnuItmDelete_Click(object sender, RoutedEventArgs e) => Source = null;

        /// Load
        private void MnuItmLoad_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog()
            {
                Filter = "All files (*.*)|*.*|BMP (*.bmp)|*.bmp|JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|GIF (*.gif)|*.gif|ICO (*.ico)|*.ico|PNG (*.png)|*.png"
            };
            if (dialog.ShowDialog() == true)
                Source = Fn.LoadImage(File.ReadAllBytes(dialog.FileName));
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
}
