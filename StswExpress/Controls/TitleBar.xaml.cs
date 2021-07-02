using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace StswExpress
{
    /// <summary>
    /// Interaction logic for TitleBar.xaml
    /// </summary>
    public partial class TitleBar : DockPanel
    {
        private Window win;
        private double height = 450, width = 700;

        public TitleBar()
        {
            InitializeComponent();
        }

        /// <summary>
        /// SubIcon
        /// </summary>
        public static readonly DependencyProperty SubIconProperty
            = DependencyProperty.Register(
                  nameof(SubIcon),
                  typeof(ImageSource),
                  typeof(TitleBar),
                  new PropertyMetadata(default(ImageSource))
              );
        public ImageSource SubIcon
        {
            get => (ImageSource)GetValue(SubIconProperty);
            set => SetValue(SubIconProperty, value);
        }

        /// <summary>
        /// Loaded
        /// </summary>
        private void TitleBar_Loaded(object sender, RoutedEventArgs e)
        {
            win = Window.GetWindow(this);
            if (win != null)
            {
                if (!win.SizeToContent.In(SizeToContent.WidthAndHeight, SizeToContent.Width))
                {
                    width = win.Width;
                    if (win.MinWidth == 0)
                        win.MinWidth = width * 0.4;
                }
                if (!win.SizeToContent.In(SizeToContent.WidthAndHeight, SizeToContent.Height))
                {
                    height = win.Height;
                    if (win.MinHeight == 0)
                        win.MinHeight = height * 0.4;
                }

                if (win.BorderThickness.Top == 0)
                {
                    win.BorderThickness = win.ResizeMode == ResizeMode.CanResize ? new Thickness(6) : new Thickness(3);
                    var col = (Color)ColorConverter.ConvertFromString(Settings.Default.ThemeColor);
                    win.BorderBrush = new SolidColorBrush(new Color
                    {
                        R = (byte)(col.R * 0.75),
                        G = (byte)(col.G * 0.75),
                        B = (byte)(col.B * 0.75),
                        A = byte.MaxValue
                    });
                }
                win.WindowStyle = WindowStyle.None;

                if (!string.IsNullOrEmpty(Settings.Default.iFont))
                    win.SetBinding(Control.FontFamilyProperty, new Binding()
                    {
                        Path = new PropertyPath("iFont"),
                        Source = Settings.Default
                    });
                if (Settings.Default.iSize > 0)
                    win.SetBinding(Control.FontSizeProperty, new Binding()
                    {
                        Path = new PropertyPath("iSize"),
                        Source = Settings.Default
                    });

                MnuItmDefaultSize.IsEnabled = win.ResizeMode.In(ResizeMode.CanResize, ResizeMode.CanResizeWithGrip);
                MnuItmMinimize.IsEnabled = win.ResizeMode.In(ResizeMode.CanMinimize, ResizeMode.CanResize, ResizeMode.CanResizeWithGrip);
                MnuItmMaximize.IsEnabled = win.ResizeMode.In(ResizeMode.CanResize, ResizeMode.CanResizeWithGrip);

                win.StateChanged += Window_StateChanged;
                Window_StateChanged(null, null);
            }
            else win = new Window();
        }

        /// <summary>
        /// StateChanged
        /// </summary>
        private void Window_StateChanged(object sender, EventArgs e) => MnuItmMaximize.Header = win.WindowState != WindowState.Maximized ? "Maksymalizuj" : "Przywróć";

        /// <summary>
        /// MouseDown
        /// </summary>
        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                if (e.ClickCount == 2 && win.ResizeMode == ResizeMode.CanResize)
                    MnuItmResize_Click(null, null);
                else
                    win.DragMove();
            }
        }

        /// <summary>
        /// Default size
        /// </summary>
        private void MnuItmDefaultSize_Click(object sender, RoutedEventArgs e)
        {
            win.Height = height;
            win.Width = width;
            MnuItmSetCenter_Click(null, null);
        }

        /// <summary>
        /// Set center
        /// </summary>
        public void MnuItmSetCenter_Click(object sender, RoutedEventArgs e)
        {
            var workArea = SystemParameters.WorkArea;
            win.Left = (workArea.Width - win.Width) / 2 + workArea.Left;
            win.Top = (workArea.Height - win.Height) / 2 + workArea.Top;
        }

        /// <summary>
        /// Minimize
        /// </summary>
        private void MnuItmMinimize_Click(object sender, RoutedEventArgs e) => win.WindowState = WindowState.Minimized;

        /// <summary>
        /// Resize
        /// </summary>
        private void MnuItmResize_Click(object sender, RoutedEventArgs e) => win.WindowState = win.WindowState == WindowState.Normal ? WindowState.Maximized : WindowState.Normal;

        /// <summary>
        /// Close
        /// </summary>
        private void MnuItmClose_Click(object sender, RoutedEventArgs e) => win.Close();
    }
}
