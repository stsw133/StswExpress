using Microsoft.Win32;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace StswExpress;

/// <summary>
/// Interaction logic for StswImage.xaml
/// </summary>
public partial class StswImage : StswImageBase
{
    public StswImage()
    {
        InitializeComponent();
    }
    static StswImage()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswImage), new FrameworkPropertyMetadata(typeof(StswImage)));
    }
}

public class StswImageBase : UserControl
{
    /// ContextMenuVisibility
    public static readonly DependencyProperty ContextMenuVisibilityProperty
        = DependencyProperty.Register(
            nameof(ContextMenuVisibility),
            typeof(Visibility),
            typeof(StswImageBase),
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
            typeof(StswImageBase),
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
            typeof(object),
            typeof(StswImageBase),
            new PropertyMetadata(default(object?))
        );
    public object? Source
    {
        get => (object?)GetValue(SourceProperty) ?? DependencyProperty.UnsetValue;
        set => SetValue(SourceProperty, value);
    }

    /// Stretch
    public static readonly DependencyProperty StretchProperty
        = DependencyProperty.Register(
            nameof(Stretch),
            typeof(Stretch),
            typeof(StswImageBase),
            new PropertyMetadata(default(Stretch))
        );
    public Stretch Stretch
    {
        get => (Stretch)GetValue(StretchProperty);
        set => SetValue(StretchProperty, value);
    }

    /// Cut
    protected void MnuItmCut_Click(object sender, RoutedEventArgs e)
    {
        if (Source != null)
            Clipboard.SetImage(Source as BitmapSource);
        Source = null;
    }

    /// Copy
    protected void MnuItmCopy_Click(object sender, RoutedEventArgs e)
    {
        if (Source != null)
            Clipboard.SetImage(Source as BitmapSource);
    }

    /// Paste
    protected void MnuItmPaste_Click(object sender, RoutedEventArgs e)
    {
        if (Clipboard.GetImage().GetType() == typeof(InteropBitmap))
            Source = ImageFromClipboard();
        else
            Source = Clipboard.GetImage();
    }

    /// Delete
    protected void MnuItmDelete_Click(object sender, RoutedEventArgs e) => Source = null;

    /// Load
    protected void MnuItmLoad_Click(object sender, RoutedEventArgs e)
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
    protected void MnuItmSave_Click(object sender, RoutedEventArgs e)
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

    #region ImageFromClipboard
    /// ImageFromClipboardDib
    public static ImageSource ImageFromClipboard()
    {
        MemoryStream ms = Clipboard.GetData("DeviceIndependentBitmap") as MemoryStream;
        if (ms != null)
        {
            byte[] dibBuffer = new byte[ms.Length];
            ms.Read(dibBuffer, 0, dibBuffer.Length);

            BITMAPINFOHEADER infoHeader =
                BinaryStructConverter.FromByteArray<BITMAPINFOHEADER>(dibBuffer);

            int fileHeaderSize = Marshal.SizeOf(typeof(BITMAPFILEHEADER));
            int infoHeaderSize = infoHeader.biSize;
            int fileSize = fileHeaderSize + infoHeader.biSize + infoHeader.biSizeImage;

            BITMAPFILEHEADER fileHeader = new BITMAPFILEHEADER();
            fileHeader.bfType = BITMAPFILEHEADER.BM;
            fileHeader.bfSize = fileSize;
            fileHeader.bfReserved1 = 0;
            fileHeader.bfReserved2 = 0;
            fileHeader.bfOffBits = fileHeaderSize + infoHeaderSize + infoHeader.biClrUsed * 4;

            byte[] fileHeaderBytes =
                BinaryStructConverter.ToByteArray<BITMAPFILEHEADER>(fileHeader);

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
