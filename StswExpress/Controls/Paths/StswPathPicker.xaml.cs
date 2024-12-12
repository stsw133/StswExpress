using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
public class StswPathPicker : StswBoxBase
{
    static StswPathPicker()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswPathPicker), new FrameworkPropertyMetadata(typeof(StswPathPicker)));
        ToolTipService.ToolTipProperty.OverrideMetadata(typeof(StswPathPicker), new FrameworkPropertyMetadata(null, StswToolTip.OnToolTipChanged));
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
            if (SelectionUnit == StswPathType.Directory)
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
    protected override void UpdateMainProperty(bool alwaysUpdate)
    {
        if (alwaysUpdate)
        {
            var bindingExpression = GetBindingExpression(TextProperty);
            if (bindingExpression != null && bindingExpression.Status.In(BindingStatus.Active, BindingStatus.UpdateSourceError))
                bindingExpression.UpdateSource();
        }
    }

    /// <summary>
    /// Handles the Click event for the file dialog button, triggering a file or directory selection dialog.
    /// </summary>
    /// <param name="sender">The sender object triggering the event</param>
    /// <param name="e">The event arguments</param>
    private void PART_DialogButton_Click(object sender, RoutedEventArgs e)
    {
        if (SelectionUnit == StswPathType.Directory)
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

    /// <summary>
    /// Generates a textual representation of the file size to display it as a string.
    /// </summary>
    /// <param name="filePath">Path to file</param>
    /// <returns>A textual representation of the file size in one of the following units: B, KB, MB, GB.</returns>
    public static string DisplayFileSize(string filePath)
    {
        var length = new FileInfo(filePath).Length;
        return length switch
        {
            < 1_024 => $"{length} B",
            < 1_048_576 => $"{length / 1_024} KB",
            < 1_073_741_824 => $"{length / 1_048_576} MB",
            _ => $"{length / 1_073_741_824} GB"
        };
    }
    #endregion

    #region Logic properties
    /// <summary>
    /// Gets or sets the file icon source for the file picker.
    /// </summary>
    public ImageSource? FileIcon
    {
        get => (ImageSource?)GetValue(FileIconProperty);
        internal set => SetValue(FileIconProperty, value);
    }
    public static readonly DependencyProperty FileIconProperty
        = DependencyProperty.Register(
            nameof(FileIcon),
            typeof(ImageSource),
            typeof(StswPathPicker)
        );

    /// <summary>
    /// Gets or sets the info about file's length.
    /// </summary>
    internal string? FileSize
    {
        get => (string?)GetValue(FileSizeProperty);
        private set => SetValue(FileSizeProperty, value);
    }
    internal static readonly DependencyProperty FileSizeProperty
        = DependencyProperty.Register(
            nameof(FileSize),
            typeof(string),
            typeof(StswPathPicker)
        );

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
            typeof(StswPathPicker)
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
            typeof(StswPathPicker),
            new FrameworkPropertyMetadata(default(bool),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnIsShiftingEnabledChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    public static void OnIsShiftingEnabledChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswPathPicker stsw)
        {
            stsw.ListAdjacentPaths();
        }
    }

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
            typeof(StswPathPicker),
            new FrameworkPropertyMetadata(default(string?),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnSelectedPathChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    public static void OnSelectedPathChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswPathPicker stsw)
        {
            stsw.FileSize = File.Exists(stsw.SelectedPath) ? DisplayFileSize(stsw.SelectedPath) : null;
            stsw.FileIcon = StswFn.ExtractAssociatedIcon(stsw.SelectedPath)?.ToImageSource();

            /// load adjacent paths
            if (Path.Exists(stsw.SelectedPath) && Directory.GetParent(stsw.SelectedPath!)?.FullName is string parentPath && parentPath != stsw.parentPath)
            {
                stsw.parentPath = parentPath;
                stsw.ListAdjacentPaths();
            }

            /// event for non MVVM programming
            stsw.SelectedPathChanged?.Invoke(stsw, new StswValueChangedEventArgs<string?>((string?)e.OldValue, (string?)e.NewValue));
        }
    }
    private string? parentPath;

    /// <summary>
    /// Gets or sets the type of paths that can be selected (File or Directory).
    /// </summary>
    public StswPathType SelectionUnit
    {
        get => (StswPathType)GetValue(SelectionUnitProperty);
        set => SetValue(SelectionUnitProperty, value);
    }
    public static readonly DependencyProperty SelectionUnitProperty
        = DependencyProperty.Register(
            nameof(SelectionUnit),
            typeof(StswPathType),
            typeof(StswPathPicker),
            new FrameworkPropertyMetadata(default(StswPathType), OnIsShiftingEnabledChanged)
        );
    #endregion

    #region Style properties
    /// <summary>
    /// Gets or sets whether to show or not file size.
    /// </summary>
    public bool IsFileSizeVisible
    {
        get => (bool)GetValue(IsFileSizeVisibleProperty);
        set => SetValue(IsFileSizeVisibleProperty, value);
    }
    public static readonly DependencyProperty IsFileSizeVisibleProperty
        = DependencyProperty.Register(
            nameof(IsFileSizeVisible),
            typeof(bool),
            typeof(StswPathPicker)
        );
    #endregion
}
