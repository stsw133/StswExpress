using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace StswExpress.Controls
{
    /// <summary>
    /// Interaction logic for TitleBar.xaml
    /// </summary>
    public partial class TitleBar : UserControl
    {
		Window win;
		double height, width;

        public TitleBar()
        {
            InitializeComponent();
		}

		/// <summary>
		/// SubIcon
		/// </summary>
		public ImageSource SubIcon
		{
			get { return (ImageSource)GetValue(pSubIcon); }
			set { SetValue(pSubIcon, value); }
		}
		public static readonly DependencyProperty pSubIcon
			= DependencyProperty.Register(
				  nameof(SubIcon),
				  typeof(ImageSource),
				  typeof(TitleBar),
				  new PropertyMetadata(null)
			  );

		/// <summary>
		/// Loaded
		/// </summary>
		private void UserControl_Loaded(object sender, RoutedEventArgs e)
		{
			win = Window.GetWindow(this);
			height = win?.Height ?? 450;
			width = win?.Width ?? 700;
		}

		/// <summary>
		/// MouseDown
		/// </summary>
		private void titleBar_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (e.ChangedButton == MouseButton.Left)
			{
				if (e.ClickCount == 2 && btnResize.IsEnabled)
					miResize_Click(null, null);
				else
					win.DragMove();
			}
		}

		/// <summary>
		/// Default size
		/// </summary>
		private void miDefaultSize_Click(object sender, RoutedEventArgs e)
		{
			win.Height = height;
			win.Width = width;
		}

		/// <summary>
		/// Set center
		/// </summary>
		public void miSetCenter_Click(object sender, RoutedEventArgs e)
		{
			Rect workArea = SystemParameters.WorkArea;
			win.Left = (workArea.Width - win.Width) / 2 + workArea.Left;
			win.Top = (workArea.Height - win.Height) / 2 + workArea.Top;
		}

		/// <summary>
		/// Minimize
		/// </summary>
		private void miMinimize_Click(object sender, RoutedEventArgs e)
		{
			win.WindowState = WindowState.Minimized;
		}

		/// <summary>
		/// Resize
		/// </summary>
		private void miResize_Click(object sender, RoutedEventArgs e)
		{
			if (win.WindowState == WindowState.Normal)
				win.WindowState = WindowState.Maximized;
			else
				win.WindowState = WindowState.Normal;
		}

		/// <summary>
		/// Close
		/// </summary>
		private void miClose_Click(object sender, RoutedEventArgs e)
		{
			win.Close();
		}
	}
}
