using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace StswExpress;

/// <summary>
/// Represents a control to display image with additional features such as context menu options.
/// </summary>
public class StswImage : UserControl
{
    static StswImage()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswImage), new FrameworkPropertyMetadata(typeof(StswImage)));
    }

    #region Events and methods
    /// <summary>
    /// Occurs when the template is applied to the control.
    /// </summary>
    public override void OnApplyTemplate()
    {
        /// MainBorder: context menu opening
        if (GetTemplateChild("PART_MainBorder") is Border mainBorder)
            mainBorder.ContextMenuOpening += PART_MainBorder_ContextMenuOpening;

        /// Menu: cut
        if (GetTemplateChild("PART_Cut") is MenuItem mniCut)
            mniCut.Click += MniCut_Click;
        /// Menu: copy
        if (GetTemplateChild("PART_Copy") is MenuItem mniCopy)
            mniCopy.Click += MniCopy_Click;
        /// Menu: paste
        if (GetTemplateChild("PART_Paste") is MenuItem mniPaste)
            mniPaste.Click += MniPaste_Click;
        /// Menu: delete
        if (GetTemplateChild("PART_Delete") is MenuItem mniDelete)
            mniDelete.Click += MniDelete_Click;
        /// Menu: load
        if (GetTemplateChild("PART_Load") is MenuItem mniLoad)
            mniLoad.Click += MniLoad_Click;
        /// Menu: save
        if (GetTemplateChild("PART_Save") is MenuItem mniSave)
            mniSave.Click += MniSave_Click;

        base.OnApplyTemplate();
        //UpdateLayout();
    }

    /// <summary>
    /// Occurs when the context menu is opening.
    /// </summary>
    private void PART_MainBorder_ContextMenuOpening(object sender, ContextMenuEventArgs e)
    {
        ((MenuItem)GetTemplateChild("PART_Cut")).IsEnabled = MenuMode == StswMenuMode.Full;
        ((MenuItem)GetTemplateChild("PART_Copy")).IsEnabled = MenuMode != StswMenuMode.Disabled;
        ((MenuItem)GetTemplateChild("PART_Paste")).IsEnabled = MenuMode == StswMenuMode.Full;
        ((MenuItem)GetTemplateChild("PART_Delete")).IsEnabled = MenuMode == StswMenuMode.Full;
        ((MenuItem)GetTemplateChild("PART_Load")).IsEnabled = MenuMode == StswMenuMode.Full;
        ((MenuItem)GetTemplateChild("PART_Save")).IsEnabled = MenuMode != StswMenuMode.Disabled;
    }

    /// <summary>
    /// Occurs when the "Cut" menu item is clicked.
    /// </summary>
    private void MniCut_Click(object sender, RoutedEventArgs e)
    {
        if (Source != null)
            Clipboard.SetImage(Source as BitmapSource);
        Source = null;
    }

    /// <summary>
    /// Occurs when the "Copy" menu item is clicked.
    /// </summary>
    private void MniCopy_Click(object sender, RoutedEventArgs e)
    {
        if (Source != null)
            Clipboard.SetImage(Source as BitmapSource);
    }

    /// <summary>
    /// Occurs when the "Paste" menu item is clicked.
    /// </summary>
    private void MniPaste_Click(object sender, RoutedEventArgs e)
    {
        if (Clipboard.ContainsImage())
            Source = Clipboard.GetImage();

        //if (Clipboard.GetImage()?.GetType() == typeof(InteropBitmap))
        //    Source = ImageFromClipboard();
        //else
        //    Source = Clipboard.GetImage();
    }

    /// <summary>
    /// Occurs when the "Delete" menu item is clicked.
    /// </summary>
    private void MniDelete_Click(object sender, RoutedEventArgs e) => Source = null;

    /// <summary>
    /// Occurs when the "Load" menu item is clicked.
    /// </summary>
    private void MniLoad_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog()
        {
            Filter = "All files (*.*)|*.*|BMP (*.bmp)|*.bmp|JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|GIF (*.gif)|*.gif|ICO (*.ico)|*.ico|PNG (*.png)|*.png"
        };
        try
        {
            if (dialog.ShowDialog() == true)
                Source = File.ReadAllBytes(dialog.FileName).ToBitmapImage();
        }
        catch { }
    }

    /// <summary>
    /// Occurs when the "Save" menu item is clicked.
    /// </summary>
    private void MniSave_Click(object sender, RoutedEventArgs e)
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
    
    #region Main properties
    /// <summary>
    /// Gets or sets the menu mode for the control.
    /// </summary>
    public StswMenuMode MenuMode
    {
        get => (StswMenuMode)GetValue(MenuModeProperty);
        set => SetValue(MenuModeProperty, value);
    }
    public static readonly DependencyProperty MenuModeProperty
        = DependencyProperty.Register(
            nameof(MenuMode),
            typeof(StswMenuMode),
            typeof(StswImage)
        );

    /// <summary>
    /// Gets or sets the scale of the image.
    /// </summary>
    public GridLength Scale
    {
        get => (GridLength)GetValue(ScaleProperty);
        set => SetValue(ScaleProperty, value);
    }
    public static readonly DependencyProperty ScaleProperty
        = DependencyProperty.Register(
            nameof(Scale),
            typeof(GridLength),
            typeof(StswImage),
            new PropertyMetadata(default(GridLength), OnScaleChanged)
        );
    public static void OnScaleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswImage stsw)
        {
            stsw.Height = stsw.Scale == GridLength.Auto ? double.NaN : stsw.Scale.Value * 12;
            stsw.Width = stsw.Scale == GridLength.Auto ? double.NaN : stsw.Scale.Value * 12;
        }
    }

    /// <summary>
    /// Gets or sets the <see cref="ImageSource"/> of the control.
    /// </summary>
    public ImageSource? Source
    {
        get => (ImageSource?)GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }
    public static readonly DependencyProperty SourceProperty
        = DependencyProperty.Register(
            nameof(Source),
            typeof(ImageSource),
            typeof(StswImage)
        );

    /// <summary>
    /// Gets or sets the stretch behavior of the control.
    /// </summary>
    public Stretch Stretch
    {
        get => (Stretch)GetValue(StretchProperty);
        set => SetValue(StretchProperty, value);
    }
    public static readonly DependencyProperty StretchProperty
        = DependencyProperty.Register(
            nameof(Stretch),
            typeof(Stretch),
            typeof(StswImage)
        );
    #endregion

    /*
    #region ImageFromClipboard
    /// ImageFromClipboardDib
    public static ImageSource? ImageFromClipboard()
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
                object? obj = Marshal.PtrToStructure(ptr, typeof(T));
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
    */
}
