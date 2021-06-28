using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace StswExpress
{
    /// <summary>
    /// Interaction logic for Header.xaml
    /// </summary>
    public partial class Header : StackPanel
    {
        public Header()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Icon
        /// </summary>
        public static readonly DependencyProperty IconProperty
            = DependencyProperty.Register(
                  nameof(Icon),
                  typeof(ImageSource),
                  typeof(Header),
                  new PropertyMetadata(default(ImageSource))
              );
        public ImageSource Icon
        {
            get => (ImageSource)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        /// <summary>
        /// IconMargin
        /// </summary>
        public static readonly DependencyProperty IconMarginProperty
            = DependencyProperty.Register(
                  nameof(IconMargin),
                  typeof(double),
                  typeof(Header),
                  new PropertyMetadata(Settings.Default.iSize * 0.15)
              );
        public double IconMargin
        {
            get => (double)GetValue(IconMarginProperty);
            set => SetValue(IconMarginProperty, value);
        }

        /// <summary>
        /// IconSize
        /// </summary>
        public static readonly DependencyProperty IconSizeProperty
            = DependencyProperty.Register(
                  nameof(IconSize),
                  typeof(double),
                  typeof(Header),
                  new PropertyMetadata(Settings.Default.iSize * 1.5)
              );
        public double IconSize
        {
            get => (double)GetValue(IconSizeProperty);
            set => SetValue(IconSizeProperty, value);
        }

        /// <summary>
        /// IsTextVisible
        /// </summary>
        public static readonly DependencyProperty IsTextVisibleProperty
            = DependencyProperty.Register(
                  nameof(IsTextVisible),
                  typeof(bool),
                  typeof(Header),
                  new PropertyMetadata(true)
              );
        public bool IsTextVisible
        {
            get => (bool)GetValue(IsTextVisibleProperty);
            set => SetValue(IsTextVisibleProperty, value);
        }

		/// <summary>
		/// LabelPadding
		/// </summary>
		public static readonly DependencyProperty LabelPaddingProperty
			= DependencyProperty.Register(
				  nameof(LabelPadding),
				  typeof(Thickness),
				  typeof(Header),
				  new PropertyMetadata(new Thickness(5))
			  );
		public Thickness LabelPadding
		{
			get => (Thickness)GetValue(LabelPaddingProperty);
			set => SetValue(LabelPaddingProperty, value);
		}

		/// <summary>
		/// SubIcon
		/// </summary>
		public static readonly DependencyProperty SubIconProperty
			= DependencyProperty.Register(
				  nameof(SubIcon),
				  typeof(ImageSource),
				  typeof(Header),
				  new PropertyMetadata(default(ImageSource))
			  );
		public ImageSource SubIcon
		{
			get => (ImageSource)GetValue(SubIconProperty);
			set => SetValue(SubIconProperty, value);
		}

		/// <summary>
		/// SubText
		/// </summary>
		public static readonly DependencyProperty SubTextProperty
			= DependencyProperty.Register(
				  nameof(SubText),
				  typeof(string),
				  typeof(Header),
				  new PropertyMetadata(default(string))
			  );
		public string SubText
		{
			get => (string)GetValue(SubTextProperty);
			set => SetValue(SubTextProperty, value);
		}

		/// <summary>
		/// Text
		/// </summary>
		public static readonly DependencyProperty TextProperty
            = DependencyProperty.Register(
                  nameof(Text),
                  typeof(string),
                  typeof(Header),
                  new PropertyMetadata(default(string))
              );
        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }
    }
}
