using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;

namespace StswExpress;

/// <summary>
/// A file and directory path selection control with a built-in file dialog.
/// Supports file filtering, multi-selection, and displaying file sizes.
/// </summary>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;se:StswPathPicker SelectedPath="{Binding FilePath}" SelectionUnit="OpenFile" Filter="Text Files|*.txt"/&gt;
/// </code>
/// </example>
[ContentProperty(nameof(SelectedPath))]
[StswInfo("0.5.0")]
public class StswPathPicker : StswBoxBase
{
    static StswPathPicker()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswPathPicker), new FrameworkPropertyMetadata(typeof(StswPathPicker)));
    }

    #region Events & methods
    /// <inheritdoc/>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        /// Button: open dialog window
        if (GetTemplateChild("PART_DialogButton") is ButtonBase btnDialog)
            btnDialog.Click += PART_DialogButton_Click;

        AttachTextValidationRule();
        ListAdjacentPaths();
    }

    /// <inheritdoc/>
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
    /// Attaches a validation rule to the Text property to ensure the path exists based on the selection unit.
    /// </summary>
    [StswInfo("0.21.0")]
    private void AttachTextValidationRule()
    {
        var current = BindingOperations.GetBinding(this, TextProperty);

        var b = new Binding
        {
            Path = current?.Path ?? new PropertyPath(nameof(SelectedPath)),
            Mode = current?.Mode ?? BindingMode.TwoWay,
            RelativeSource = current?.RelativeSource ?? new RelativeSource(RelativeSourceMode.Self),
            TargetNullValue = current?.TargetNullValue ?? string.Empty,
            StringFormat = current?.StringFormat,
            UpdateSourceTrigger = UpdateSourceTrigger.Explicit
        };
        b.ValidationRules.Add(new StswPathExistsValidationRule { Host = this });

        BindingOperations.SetBinding(this, TextProperty, b);
    }

    /// <summary>
    /// Lists adjacent paths based on the current selected path and path type.
    /// The paths listed depend on whether the selection unit is a directory or a file.
    /// </summary>
    [StswInfo("0.5.0", "0.21.0")]
    private void ListAdjacentPaths()
    {
        if (!IsShiftingEnabled || parentPath is null)
        {
            adjacentPaths = null;
            return;
        }

        try
        {
            if (SelectionUnit == StswPathType.OpenDirectory)
            {
                adjacentPaths = [.. Directory.GetDirectories(parentPath)];
            }
            else
            {
                var all = Directory.GetFiles(parentPath);
                var allowedExts = ParseFilterExtensions(Filter);
                adjacentPaths = [.. all.Where(p => IsFileAllowedByFilter(p, allowedExts))];
            }
        }
        catch
        {
            // e.g. UnauthorizedAccessException, PathTooLongException
            adjacentPaths = null;
        }
    }
    private IList<string>? adjacentPaths;

    /// <summary>
    /// Shifts the selected path by a specified step, updating it with an adjacent path.
    /// This allows for navigating between files or directories in the current folder.
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

    /// <inheritdoc/>
    [StswInfo("0.5.0", "0.21.0")]
    protected override void UpdateMainProperty(bool alwaysUpdate)
    {
        var isInvalid = false;
        var isPlain = false;

        var nextSelectedPath = SelectedPath;

        if (string.IsNullOrWhiteSpace(Text))
        {
            nextSelectedPath = null;
        }
        else
        {
            var raw = Environment.ExpandEnvironmentVariables(Text).Trim().Trim('"');

            try
            {
                var normalized = Path.GetFullPath(raw);
                switch (SelectionUnit)
                {
                    case StswPathType.OpenDirectory:
                        if (Directory.Exists(normalized))
                        {
                            isPlain = true;
                            nextSelectedPath = normalized;
                        }
                        else isInvalid = true;
                        break;

                    case StswPathType.OpenFile:
                        if (File.Exists(normalized))
                        {
                            isPlain = true;
                            nextSelectedPath = normalized;
                        }
                        else isInvalid = true;
                        break;

                    case StswPathType.SaveFile:
                        var dir = Path.GetDirectoryName(normalized);
                        var fileName = Path.GetFileName(normalized);

                        var dirOk = !string.IsNullOrEmpty(dir) && Directory.Exists(dir);
                        var fileNameOk = !string.IsNullOrWhiteSpace(fileName) && fileName.IndexOfAny(Path.GetInvalidFileNameChars()) < 0;

                        if (dirOk && fileNameOk)
                        {
                            isPlain = true;
                            nextSelectedPath = normalized;
                        }
                        else isInvalid = true;
                        break;

                    default:
                        isInvalid = true;
                        break;
                }
            }
            catch
            {
                isInvalid = true;
            }
        }

        if (!Equals(nextSelectedPath, SelectedPath) || alwaysUpdate)
        {
            SelectedPath = nextSelectedPath;

            var valueBE = GetBindingExpression(SelectedPathProperty);
            if (!isInvalid && valueBE?.Status == BindingStatus.Active)
                valueBE.UpdateSource();

            if (nextSelectedPath is null)
                SelectedPaths = [];
            else if (SelectionUnit != StswPathType.OpenDirectory)
                SelectedPaths = [nextSelectedPath];

            var textBE = GetBindingExpression(TextProperty);
            if (textBE != null && textBE.Status is BindingStatus.Active or BindingStatus.UpdateSourceError)
            {
                if (string.IsNullOrWhiteSpace(Text) || isPlain)
                    textBE.UpdateSource();
                else if (isInvalid && alwaysUpdate)
                    textBE.UpdateSource();
            }
        }
    }

    /// <summary>
    /// Handles the Click event for the file dialog button, triggering a file or directory selection dialog.
    /// Opens either a folder browser dialog or a file dialog depending on the selection unit.
    /// </summary>
    /// <param name="sender">The sender object triggering the event</param>
    /// <param name="e">The event arguments</param>
    private void PART_DialogButton_Click(object sender, RoutedEventArgs e)
    {
        if (SelectionUnit == StswPathType.OpenDirectory)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                SelectedPath = dialog.SelectedPath;
                SelectedPaths = [dialog.SelectedPath];
            }
        }
        else
        {
            System.Windows.Forms.FileDialog dialog = SelectionUnit switch
            {
                StswPathType.OpenFile => new System.Windows.Forms.OpenFileDialog(),
                StswPathType.SaveFile => new System.Windows.Forms.SaveFileDialog(),
                _ => throw new NotImplementedException()
            };

            /// filter
            try
            {
                dialog.Filter = Filter;
            }
            catch { }

            /// multiselect
            if (dialog is System.Windows.Forms.OpenFileDialog openFileDialog)
                openFileDialog.Multiselect = Multiselect;

            /// suggested file name
            if (SuggestedFilename != null)
                dialog.FileName = SuggestedFilename;

            /// show
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                SelectedPath = dialog.FileName;
                SelectedPaths = dialog.FileNames;
            }
        }
    }

    /// <summary>
    /// Generates a textual representation of the file size to display it as a string.
    /// The size is converted to one of the following units: B, KB, MB, or GB based on the file's size.
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

    /// <summary>
    /// Parses a file filter string and extracts the allowed file extensions.
    /// </summary>
    /// <param name="filter">The filter string in the format used by file dialogs (e.g., "Text Files|*.txt;*.md|All Files|*.*").</param>
    /// <returns>A set of allowed file extensions (including the dot), or <see langword="null"/> if no restrictions apply.</returns>
    [StswInfo("0.21.0")]
    private static HashSet<string>? ParseFilterExtensions(string filter)
    {
        if (string.IsNullOrWhiteSpace(filter))
            return null;

        var parts = filter.Split('|');
        var exts = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        for (var i = 1; i < parts.Length; i += 2)
        {
            var pat = parts[i].Trim();
            if (string.IsNullOrEmpty(pat))
                continue;

            foreach (var p in pat.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                if (p == "*.*")
                    return null;

                if (p.StartsWith("*.", StringComparison.Ordinal))
                {
                    var ext = p[1..];
                    if (ext.Length > 1) exts.Add(ext);
                }
            }
        }

        return exts.Count == 0 ? null : exts;
    }

    /// <summary>
    /// Checks if a file path is allowed based on the provided set of allowed extensions.
    /// </summary>
    /// <param name="path">The file path to check.</param>
    /// <param name="allowedExts">A set of allowed file extensions (including the dot), or <see langword="null"/> if no restrictions apply.</param>
    /// <returns><see langword="true"/> if the file is allowed, otherwise <see langword="false"/>.</returns>
    [StswInfo("0.21.0")]
    private static bool IsFileAllowedByFilter(string path, HashSet<string>? allowedExts)
    {
        if (allowedExts is null)
            return true;

        return allowedExts.Contains(Path.GetExtension(path));
    }
    #endregion

    #region Logic properties
    /// <summary>
    /// Gets or sets the file icon source for the path picker.
    /// Represents the icon to display for the selected file or directory.
    /// </summary>
    public ImageSource? FileIcon
    {
        get => (ImageSource?)GetValue(FileIconProperty);
        private set => SetValue(FileIconProperty, value);
    }
    public static readonly DependencyProperty FileIconProperty
        = DependencyProperty.Register(
            nameof(FileIcon),
            typeof(ImageSource),
            typeof(StswPathPicker)
        );

    /// <summary>
    /// Gets or sets the info about the file's length.
    /// Displays the size of the selected file, if applicable.
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
    /// Gets or sets the file filter used in the file selection dialog.
    /// Example: "Image Files (*.png;*.jpg)|*.png;*.jpg".
    /// </summary>
    [StswInfo("0.5.0", "0.21.0")]
    public string Filter
    {
        get => (string)GetValue(FilterProperty);
        set => SetValue(FilterProperty, value);
    }
    public static readonly DependencyProperty FilterProperty
        = DependencyProperty.Register(
            nameof(Filter),
            typeof(string),
            typeof(StswPathPicker),
            new FrameworkPropertyMetadata(default(string), OnFilterChanged)
        );
    private static void OnFilterChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is not StswPathPicker stsw)
            return;

        if (stsw.SelectionUnit != StswPathType.OpenDirectory)
            stsw.ListAdjacentPaths();
    }

    /// <summary>
    /// Gets or sets a value indicating whether shifting through adjacent paths is enabled.
    /// If enabled, users can navigate between directories or files in the current folder using mouse wheel or keyboard keys.
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
        if (obj is not StswPathPicker stsw)
            return;

        stsw.ListAdjacentPaths();
    }

    /// <summary>
    /// Gets or sets the multiselect behavior for open file dialog.
    /// If <see langword="true"/>, multiple files can be selected at once in the file dialog.
    /// </summary>
    [StswInfo("0.15.0")]
    public bool Multiselect
    {
        get => (bool)GetValue(MultiselectProperty);
        set => SetValue(MultiselectProperty, value);
    }
    public static readonly DependencyProperty MultiselectProperty
        = DependencyProperty.Register(
            nameof(Multiselect),
            typeof(bool),
            typeof(StswPathPicker)
        );

    /// <summary>
    /// Gets or sets the currently selected path in the control.
    /// Represents the file or directory currently selected by the user.
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
        if (obj is not StswPathPicker stsw)
            return;

        stsw.FileSize = File.Exists(stsw.SelectedPath) ? DisplayFileSize(stsw.SelectedPath) : null;
        stsw.FileIcon = StswFnUI.ExtractAssociatedIcon(stsw.SelectedPath)?.ToImageSource();

        /// load adjacent paths
        if (Path.Exists(stsw.SelectedPath) && Directory.GetParent(stsw.SelectedPath!)?.FullName is string parentPath && parentPath != stsw.parentPath)
        {
            stsw.parentPath = parentPath;
            stsw.ListAdjacentPaths();
        }
    }
    private string? parentPath;

    /// <summary>
    /// Gets or sets the currently selected path in the control.
    /// Represents the file or directory currently selected by the user.
    /// </summary>
    [StswInfo("0.15.0")]
    public string[] SelectedPaths
    {
        get => (string[])GetValue(SelectedPathsProperty);
        set => SetValue(SelectedPathsProperty, value);
    }
    public static readonly DependencyProperty SelectedPathsProperty
        = DependencyProperty.Register(
            nameof(SelectedPaths),
            typeof(string[]),
            typeof(StswPathPicker),
            new FrameworkPropertyMetadata(Array.Empty<string>())
        );

    /// <summary>
    /// Gets or sets the type of paths that can be selected (File or Directory).
    /// This determines whether the control allows selection of files or directories within the dialog.
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

    /// <summary>
    /// Gets or sets the suggested file name for file dialog default file name.
    /// Provides a default name for files when the save dialog is shown.
    /// </summary>
    [StswInfo("0.15.0")]
    public string? SuggestedFilename
    {
        get => (string?)GetValue(SuggestedFilenameProperty);
        set => SetValue(SuggestedFilenameProperty, value);
    }
    public static readonly DependencyProperty SuggestedFilenameProperty
        = DependencyProperty.Register(
            nameof(SuggestedFilename),
            typeof(string),
            typeof(StswPathPicker)
        );
    #endregion

    #region Style properties
    /// <summary>
    /// Gets or sets whether to show or not the file size.
    /// If true, the size of the selected file is displayed next to the selected path.
    /// </summary>
    [StswInfo("0.7.0")]
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
