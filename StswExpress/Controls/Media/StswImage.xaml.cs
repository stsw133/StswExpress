using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace StswExpress;
/// <summary>
/// A customizable image control that supports context menu actions such as copy, paste, load, and save.
/// </summary>
[ContentProperty(nameof(Source))]
[Stsw(null)]
public class StswImage : Control, IStswCornerControl
{
    static StswImage()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswImage), new FrameworkPropertyMetadata(typeof(StswImage)));
    }

    #region Events & methods
    /// <inheritdoc/>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        /// Menu: cut
        if (GetTemplateChild("PART_Cut") is MenuItem mniCut)
            mniCut.Click += PART_Cut_Click;
        /// Menu: copy
        if (GetTemplateChild("PART_Copy") is MenuItem mniCopy)
            mniCopy.Click += PART_Copy_Click;
        /// Menu: paste
        if (GetTemplateChild("PART_Paste") is MenuItem mniPaste)
            mniPaste.Click += PART_Paste_Click;
        /// Menu: delete
        if (GetTemplateChild("PART_Delete") is MenuItem mniDelete)
            mniDelete.Click += PART_Delete_Click;
        /// Menu: load
        if (GetTemplateChild("PART_Load") is MenuItem mniLoad)
            mniLoad.Click += PART_Load_Click;
        /// Menu: save
        if (GetTemplateChild("PART_Save") is MenuItem mniSave)
            mniSave.Click += PART_Save_Click;
    }

    /// <summary>
    /// Occurs when the "Cut" menu item is clicked.
    /// </summary>
    /// <param name="sender">The sender object triggering the event</param>
    /// <param name="e">The event arguments</param>
    private void PART_Cut_Click(object sender, RoutedEventArgs e)
    {
        if (Source != null)
            Clipboard.SetImage(Source as BitmapSource);
        Source = null;
    }

    /// <summary>
    /// Occurs when the "Copy" menu item is clicked.
    /// </summary>
    /// <param name="sender">The sender object triggering the event</param>
    /// <param name="e">The event arguments</param>
    private void PART_Copy_Click(object sender, RoutedEventArgs e)
    {
        if (Source != null)
            Clipboard.SetImage(Source as BitmapSource);
    }

    /// <summary>
    /// Occurs when the "Paste" menu item is clicked.
    /// </summary>
    /// <param name="sender">The sender object triggering the event</param>
    /// <param name="e">The event arguments</param>
    private void PART_Paste_Click(object sender, RoutedEventArgs e)
    {
        if (Clipboard.ContainsImage())
            Source = Clipboard.GetImage();
    }

    /// <summary>
    /// Occurs when the "Delete" menu item is clicked.
    /// </summary>
    /// <param name="sender">The sender object triggering the event</param>
    /// <param name="e">The event arguments</param>
    private void PART_Delete_Click(object sender, RoutedEventArgs e) => Source = null;

    /// <summary>
    /// Occurs when the "Load" menu item is clicked.
    /// </summary>
    /// <param name="sender">The sender object triggering the event</param>
    /// <param name="e">The event arguments</param>
    private void PART_Load_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog()
        {
            Filter = "All files (*.*)|*.*|BMP (*.bmp)|*.bmp|JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|GIF (*.gif)|*.gif|ICO (*.ico)|*.ico|PNG (*.png)|*.png"
        };
        try
        {
            if (dialog.ShowDialog() == true)
                Source = new BitmapImage(new Uri(dialog.FileName));
        }
        catch { }
    }

    /// <summary>
    /// Occurs when the "Save" menu item is clicked.
    /// </summary>
    /// <param name="sender">The sender object triggering the event</param>
    /// <param name="e">The event arguments</param>
    private void PART_Save_Click(object sender, RoutedEventArgs e)
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

    #region Logic properties
    /// <summary>
    /// Gets or sets the menu mode for the image control, defining how the context menu behaves.
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
    /// Gets or sets the scale factor for the image, adjusting its width and height accordingly.
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
        if (obj is not StswImage stsw)
            return;

        stsw.Height = stsw.Scale.IsStar ? double.NaN : stsw.Scale!.Value * 12;
        stsw.Width = stsw.Scale.IsStar ? double.NaN : stsw.Scale!.Value * 12;
    }

    /// <summary>
    /// Gets or sets the image source displayed in the control.
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
    /// Gets or sets the stretch mode of the image, determining how it fits within the control's boundaries.
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

    /// <summary>
    /// Gets or sets the stretch direction, specifying whether the image can scale up, down, or both.
    /// </summary>
    [Stsw("0.12.0")]
    public StretchDirection StretchDirection
    {
        get => (StretchDirection)GetValue(StretchDirectionProperty);
        set => SetValue(StretchDirectionProperty, value);
    }
    public static readonly DependencyProperty StretchDirectionProperty
        = DependencyProperty.Register(
            nameof(StretchDirection),
            typeof(StretchDirection),
            typeof(StswImage)
        );
    #endregion

    #region Style properties
    /// <inheritdoc/>
    public bool CornerClipping
    {
        get => (bool)GetValue(CornerClippingProperty);
        set => SetValue(CornerClippingProperty, value);
    }
    public static readonly DependencyProperty CornerClippingProperty
        = DependencyProperty.Register(
            nameof(CornerClipping),
            typeof(bool),
            typeof(StswImage),
            new FrameworkPropertyMetadata(default(bool), FrameworkPropertyMetadataOptions.AffectsRender)
        );

    /// <inheritdoc/>
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }
    public static readonly DependencyProperty CornerRadiusProperty
        = DependencyProperty.Register(
            nameof(CornerRadius),
            typeof(CornerRadius),
            typeof(StswImage),
            new FrameworkPropertyMetadata(default(CornerRadius), FrameworkPropertyMetadataOptions.AffectsRender)
        );
    #endregion
}

/* usage:

<se:StswImage Source="example.png" Stretch="Uniform"/>

*/
