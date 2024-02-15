using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;

namespace StswExpress;

/// <summary>
/// A control that allows users to select file or directory path with additional features.
/// </summary>
[ContentProperty(nameof(SelectedPath))]
public class StswFilePicker : TextBox, IStswBoxControl, IStswCornerControl
{
    public StswFilePicker()
    {
        SetValue(SubControlsProperty, new ObservableCollection<IStswSubControl>());
    }
    static StswFilePicker()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswFilePicker), new FrameworkPropertyMetadata(typeof(StswFilePicker)));
    }

    #region Events & methods
    /// <summary>
    /// Occurs when the selected path in the control changes.
    /// </summary>
    public event EventHandler? SelectedPathChanged;

    /// <summary>
    /// Occurs when the template is applied to the control.
    /// </summary>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        /// Button: open dialog window
        if (GetTemplateChild("PART_DialogButton") is ButtonBase btnDialog)
            btnDialog.Click += PART_DialogButton_Click;

        ListAdjacentPaths();
    }

    /// <summary>
    /// Handles the KeyDown event for the internal content host of the file picker.
    /// If the Enter key is pressed, the LostFocus event is triggered for the content host.
    /// </summary>
    /// <param name="e">The event arguments</param>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (!AcceptsReturn && e.Key == Key.Enter)
            UpdateMainProperty();
    }

    /// <summary>
    /// Handles the MouseWheel event for the internal content host of the file picker.
    /// Adjusts the selected path based on the mouse wheel's scrolling direction.
    /// </summary>
    /// <param name="e">The event arguments</param>
    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        base.OnMouseWheel(e);

        if (IsKeyboardFocused && !IsReadOnly && IsShiftingEnabled)
        {
            ShiftBy(e.Delta > 0 ? -1 : 1);

            e.Handled = true;
        }
    }

    /// <summary>
    /// Lists adjacent paths based on the current selected path and path type.
    /// </summary>
    private void ListAdjacentPaths()
    {
        if (IsShiftingEnabled && parentPath != null)
        {
            if (PathType == StswPathType.Directory)
                adjacentPaths = Directory.GetDirectories(parentPath).ToList();
            else
                adjacentPaths = Directory.GetFiles(parentPath).ToList();
        }
        else adjacentPaths = null;
    }
    private IList<string>? adjacentPaths;

    /// <summary>
    /// Shifts the selected path by a specified step, updating it with an adjacent path.
    /// </summary>
    /// <param name="step">The step value for shifting the path by</param>
    private void ShiftBy(int step)
    {
        if (!string.IsNullOrEmpty(SelectedPath) && adjacentPaths?.Count > 0)
        {
            var currIndex = adjacentPaths.IndexOf(SelectedPath);

            if ((currIndex + step).Between(0, adjacentPaths.Count - 1))
                SelectedPath = adjacentPaths[adjacentPaths.IndexOf(SelectedPath) + step];
            else if (currIndex + step < 0)
                SelectedPath = adjacentPaths[0];
            else
                SelectedPath = adjacentPaths[adjacentPaths.Count - 1];
        }
    }

    /// <summary>
    /// Updates the main property associated with the selected path in the control based on user input.
    /// </summary>
    private void UpdateMainProperty()
    {
        var bindingExpression = GetBindingExpression(TextProperty);
        if (bindingExpression != null && bindingExpression.Status.In(BindingStatus.Active/*, BindingStatus.UpdateSourceError*/))
            bindingExpression.UpdateSource();
    }

    /// <summary>
    /// Handles the Click event for the file dialog button, triggering a file or directory selection dialog.
    /// </summary>
    /// <param name="sender">The sender object triggering the event</param>
    /// <param name="e">The event arguments</param>
    private void PART_DialogButton_Click(object sender, RoutedEventArgs e)
    {
        if (PathType == StswPathType.Directory)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                SelectedPath = dialog.SelectedPath;
        }
        else
        {
            var dialog = new System.Windows.Forms.OpenFileDialog()
            {
                Filter = Filter
            };
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                SelectedPath = dialog.FileName;
        }
    }
    #endregion

    #region Main properties
    /// <summary>
    /// Gets or sets the filter string for file dialog filters.
    /// </summary>
    public string Filter
    {
        get => (string)GetValue(FilterProperty);
        set => SetValue(FilterProperty, value);
    }
    public static readonly DependencyProperty FilterProperty
        = DependencyProperty.Register(
            nameof(Filter),
            typeof(string),
            typeof(StswFilePicker)
        );

    /// <summary>
    /// Gets or sets the icon source for the file picker.
    /// </summary>
    public ImageSource? IconSource
    {
        get => (ImageSource?)GetValue(IconSourceProperty);
        set => SetValue(IconSourceProperty, value);
    }
    public static readonly DependencyProperty IconSourceProperty
        = DependencyProperty.Register(
            nameof(IconSource),
            typeof(ImageSource),
            typeof(StswFilePicker)
        );

    /// <summary>
    /// Gets or sets a value indicating whether shifting through adjacent paths is enabled.
    /// </summary>
    public bool IsShiftingEnabled
    {
        get => (bool)GetValue(IsShiftingEnabledProperty);
        set => SetValue(IsShiftingEnabledProperty, value);
    }
    public static readonly DependencyProperty IsShiftingEnabledProperty
        = DependencyProperty.Register(
            nameof(IsShiftingEnabled),
            typeof(bool),
            typeof(StswFilePicker),
            new FrameworkPropertyMetadata(default(bool),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnIsShiftingEnabledChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    public static void OnIsShiftingEnabledChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswFilePicker stsw)
        {
            stsw.ListAdjacentPaths();
        }
    }

    /// <summary>
    /// Gets or sets the type of paths that can be selected (File or Directory).
    /// </summary>
    public StswPathType PathType
    {
        get => (StswPathType)GetValue(PathTypeProperty);
        set => SetValue(PathTypeProperty, value);
    }
    public static readonly DependencyProperty PathTypeProperty
        = DependencyProperty.Register(
            nameof(PathType),
            typeof(StswPathType),
            typeof(StswFilePicker),
            new FrameworkPropertyMetadata(default(StswPathType), OnIsShiftingEnabledChanged)
        );

    /// <summary>
    /// Gets or sets the placeholder text to display in the box when no path is selected.
    /// </summary>
    public string? Placeholder
    {
        get => (string?)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }
    public static readonly DependencyProperty PlaceholderProperty
        = DependencyProperty.Register(
            nameof(Placeholder),
            typeof(string),
            typeof(StswFilePicker)
        );

    /// <summary>
    /// Gets or sets the currently selected path in the control.
    /// </summary>
    public string? SelectedPath
    {
        get => (string?)GetValue(SelectedPathProperty);
        set => SetValue(SelectedPathProperty, value);
    }
    public static readonly DependencyProperty SelectedPathProperty
        = DependencyProperty.Register(
            nameof(SelectedPath),
            typeof(string),
            typeof(StswFilePicker),
            new FrameworkPropertyMetadata(default(string?),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnSelectedPathChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    public static void OnSelectedPathChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswFilePicker stsw)
        {
            stsw.IconSource = null;

            if (stsw.PathType == StswPathType.Directory ? Directory.Exists(stsw.SelectedPath) : File.Exists(stsw.SelectedPath))
            {
                /// path icon
                if (stsw.PathType == StswPathType.Directory)
                {
                    var shinfo = new SHFILEINFO();
                    if (SHGetFileInfo(stsw.SelectedPath, 0, ref shinfo, (uint)Marshal.SizeOf(shinfo), SHGFI_ICON | SHGFI_LARGEICON) != IntPtr.Zero)
                        stsw.IconSource = Icon.FromHandle(shinfo.hIcon).ToBitmap().ToImageSource();
                }
                else
                {
                    if (Icon.ExtractAssociatedIcon(stsw.SelectedPath) is Icon icon)
                        stsw.IconSource = icon.ToBitmap().ToImageSource();
                }

                /// load adjacent paths
                if (Directory.GetParent(stsw.SelectedPath!)?.FullName is string parentPath && parentPath != stsw.parentPath)
                {
                    stsw.parentPath = parentPath;
                    stsw.ListAdjacentPaths();
                }
            }

            stsw.SelectedPathChanged?.Invoke(stsw, EventArgs.Empty);
        }
    }
    private string? parentPath;

    /// <summary>
    /// Gets or sets the collection of sub controls to be displayed in the control.
    /// </summary>
    public ObservableCollection<IStswSubControl> SubControls
    {
        get => (ObservableCollection<IStswSubControl>)GetValue(SubControlsProperty);
        set => SetValue(SubControlsProperty, value);
    }
    public static readonly DependencyProperty SubControlsProperty
        = DependencyProperty.Register(
            nameof(SubControls),
            typeof(ObservableCollection<IStswSubControl>),
            typeof(StswFilePicker)
        );

    ///// <summary>
    ///// Gets or sets the text value of the control.
    ///// </summary>
    //[Browsable(false)]
    ////[Bindable(false)]
    //[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    //[EditorBrowsable(EditorBrowsableState.Never)]
    //public new string? Text
    //{
    //    get => base.Text;
    //    internal set => base.Text = value;
    //}
    #endregion

    #region Style properties
    /// <summary>
    /// Gets or sets a value indicating whether corner clipping is enabled for the control.
    /// When set to <see langword="true"/>, content within the control's border area is clipped to match
    /// the border's rounded corners, preventing elements from protruding beyond the border.
    /// </summary>
    public bool CornerClipping
    {
        get => (bool)GetValue(CornerClippingProperty);
        set => SetValue(CornerClippingProperty, value);
    }
    public static readonly DependencyProperty CornerClippingProperty
        = DependencyProperty.Register(
            nameof(CornerClipping),
            typeof(bool),
            typeof(StswFilePicker)
        );

    /// <summary>
    /// Gets or sets the degree to which the corners of the control's border are rounded by defining
    /// a radius value for each corner independently. This property allows users to control the roundness
    /// of corners, and large radius values are smoothly scaled to blend from corner to corner.
    /// </summary>
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }
    public static readonly DependencyProperty CornerRadiusProperty
        = DependencyProperty.Register(
            nameof(CornerRadius),
            typeof(CornerRadius),
            typeof(StswFilePicker)
        );

    /// <summary>
    /// Gets or sets a value indicating whether the error sub control is visible within the box when there is at least one validation error.
    /// </summary>
    public bool IsErrorVisible
    {
        get => (bool)GetValue(IsErrorVisibleProperty);
        set => SetValue(IsErrorVisibleProperty, value);
    }
    public static readonly DependencyProperty IsErrorVisibleProperty
        = DependencyProperty.Register(
            nameof(IsErrorVisible),
            typeof(bool),
            typeof(StswFilePicker)
        );

    /// <summary>
    /// Gets or sets the thickness of the separator between box and drop-down button.
    /// </summary>
    public double SeparatorThickness
    {
        get => (double)GetValue(SeparatorThicknessProperty);
        set => SetValue(SeparatorThicknessProperty, value);
    }
    public static readonly DependencyProperty SeparatorThicknessProperty
        = DependencyProperty.Register(
            nameof(SeparatorThickness),
            typeof(double),
            typeof(StswFilePicker)
        );
    #endregion

    const uint SHGFI_ICON = 0x100;
    const uint SHGFI_LARGEICON = 0x0; // Large icon

    [DllImport("shell32.dll")]
    static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbSizeFileInfo, uint uFlags);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    internal struct SHFILEINFO
    {
        public IntPtr hIcon;
        public int iIcon;
        public uint dwAttributes;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string szDisplayName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
        public string szTypeName;
    }
}
