using Microsoft.Win32;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace StswExpress;

/// <summary>
/// Interaction logic for StswImage.xaml
/// </summary>
public partial class StswImage : UserControl
{
    public StswImage()
    {
        InitializeComponent();
    }
    static StswImage()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswImage), new FrameworkPropertyMetadata(typeof(StswImage)));
    }

    #region Style
    /// OpacityDisabled
    public static readonly DependencyProperty OpacityDisabledProperty
        = DependencyProperty.Register(
            nameof(OpacityDisabled),
            typeof(double),
            typeof(StswImage),
            new PropertyMetadata(default(double))
        );
    public double OpacityDisabled
    {
        get => (double)GetValue(OpacityDisabledProperty);
        set => SetValue(OpacityDisabledProperty, value);
    }
    #endregion

    #region Properties
    /// MenuMode
    public enum MenuModes
    {
        Disabled,
        ReadOnly,
        Full
    }
    public static readonly DependencyProperty MenuModeProperty
        = DependencyProperty.Register(
            nameof(MenuMode),
            typeof(MenuModes),
            typeof(StswImage),
            new PropertyMetadata(default(MenuModes))
        );
    public MenuModes MenuMode
    {
        get => (MenuModes)GetValue(MenuModeProperty);
        set => SetValue(MenuModeProperty, value);
    }

    /// Scale
    public static readonly DependencyProperty ScaleProperty
        = DependencyProperty.Register(
            nameof(Scale),
            typeof(GridLength),
            typeof(StswImage),
            new FrameworkPropertyMetadata(default(GridLength),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnScaleChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    public GridLength Scale
    {
        get => (GridLength)GetValue(ScaleProperty);
        set => SetValue(ScaleProperty, value);
    }
    public static void OnScaleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswImage stsw)
        {
            if (stsw.Scale == GridLength.Auto)
            {
                stsw.Height = double.NaN;
                stsw.Width = double.NaN;
            }
            else if (BindingOperations.GetBindingBase(stsw, HeightProperty) == null || BindingOperations.GetBindingBase(stsw, WidthProperty) == null)
            {
                var multiBinding = new MultiBinding();
                multiBinding.Bindings.Add(new Binding(nameof(Settings.Default.iSize)) { Source = Settings.Default });
                multiBinding.Bindings.Add(new Binding(nameof(Scale)) { RelativeSource = new RelativeSource(RelativeSourceMode.Self) });
                multiBinding.Converter = new conv_Calculate();
                multiBinding.ConverterParameter = "*";
                stsw.SetBinding(HeightProperty, multiBinding);
                stsw.SetBinding(WidthProperty, multiBinding);
            }
        }
    }

    /// Source
    public static readonly DependencyProperty SourceProperty
        = DependencyProperty.Register(
            nameof(Source),
            typeof(ImageSource),
            typeof(StswImage),
            new FrameworkPropertyMetadata(default(ImageSource?),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                null, null, false, UpdateSourceTrigger.PropertyChanged)
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
            typeof(StswImage),
            new PropertyMetadata(default(Stretch))
        );
    public Stretch Stretch
    {
        get => (Stretch)GetValue(StretchProperty);
        set => SetValue(StretchProperty, value);
    }
    #endregion

    #region Events
    /// PART_MainBorder_ContextMenuOpening
    private void PART_MainBorder_ContextMenuOpening(object sender, ContextMenuEventArgs e)
    {
        ((MenuItem)GetTemplateChild("PART_Cut")).IsEnabled = MenuMode == MenuModes.Full;
        ((MenuItem)GetTemplateChild("PART_Copy")).IsEnabled = MenuMode != MenuModes.Disabled;
        ((MenuItem)GetTemplateChild("PART_Paste")).IsEnabled = MenuMode == MenuModes.Full;
        ((MenuItem)GetTemplateChild("PART_Delete")).IsEnabled = MenuMode == MenuModes.Full;
        ((MenuItem)GetTemplateChild("PART_Load")).IsEnabled = MenuMode == MenuModes.Full;
        ((MenuItem)GetTemplateChild("PART_Save")).IsEnabled = MenuMode != MenuModes.Disabled;
    }

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
            Source = ImageFromClipboard();
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

    #region ImageFromClipboard
    /// ImageFromClipboardDib
    public static ImageSource ImageFromClipboard()
    {
        if (Clipboard.GetData("DeviceIndependentBitmap") is MemoryStream ms)
        {
            byte[] dibBuffer = new byte[ms.Length];
            ms.Read(dibBuffer, 0, dibBuffer.Length);

            BITMAPINFOHEADER infoHeader = BinaryStructConverter.FromByteArray<BITMAPINFOHEADER>(dibBuffer);

            int fileHeaderSize = Marshal.SizeOf(typeof(BITMAPFILEHEADER));
            int infoHeaderSize = infoHeader.biSize;
            int fileSize = fileHeaderSize + infoHeader.biSize + infoHeader.biSizeImage;

            BITMAPFILEHEADER fileHeader = new BITMAPFILEHEADER();
            fileHeader.bfType = BITMAPFILEHEADER.BM;
            fileHeader.bfSize = fileSize;
            fileHeader.bfReserved1 = 0;
            fileHeader.bfReserved2 = 0;
            fileHeader.bfOffBits = fileHeaderSize + infoHeaderSize + infoHeader.biClrUsed * 4;

            byte[] fileHeaderBytes = BinaryStructConverter.ToByteArray<BITMAPFILEHEADER>(fileHeader);

            MemoryStream msBitmap = new MemoryStream();
            msBitmap.Write(fileHeaderBytes, 0, fileHeaderSize);
            msBitmap.Write(dibBuffer, 0, dibBuffer.Length);
            msBitmap.Seek(0, SeekOrigin.Begin);

            return BitmapFrame.Create(msBitmap);
        }

        return null;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    private struct BITMAPFILEHEADER
    {
        public static readonly short BM = 0x4d42; // BM

        public short bfType;
        public int bfSize;
        public short bfReserved1;
        public short bfReserved2;
        public int bfOffBits;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct BITMAPINFOHEADER
    {
        public int biSize;
        public int biWidth;
        public int biHeight;
        public short biPlanes;
        public short biBitCount;
        public int biCompression;
        public int biSizeImage;
        public int biXPelsPerMeter;
        public int biYPelsPerMeter;
        public int biClrUsed;
        public int biClrImportant;
    }

    public static class BinaryStructConverter
    {
        public static T FromByteArray<T>(byte[] bytes) where T : struct
        {
            IntPtr ptr = IntPtr.Zero;
            try
            {
                int size = Marshal.SizeOf(typeof(T));
                ptr = Marshal.AllocHGlobal(size);
                Marshal.Copy(bytes, 0, ptr, size);
                object obj = Marshal.PtrToStructure(ptr, typeof(T));
                return (T)obj;
            }
            finally
            {
                if (ptr != IntPtr.Zero)
                    Marshal.FreeHGlobal(ptr);
            }
        }

        public static byte[] ToByteArray<T>(T obj) where T : struct
        {
            IntPtr ptr = IntPtr.Zero;
            try
            {
                int size = Marshal.SizeOf(typeof(T));
                ptr = Marshal.AllocHGlobal(size);
                Marshal.StructureToPtr(obj, ptr, true);
                byte[] bytes = new byte[size];
                Marshal.Copy(ptr, bytes, 0, size);
                return bytes;
            }
            finally
            {
                if (ptr != IntPtr.Zero)
                    Marshal.FreeHGlobal(ptr);
            }
        }
    }
    #endregion
}
