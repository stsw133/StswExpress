using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;

namespace StswExpress.Wpf;

/// <summary>
/// A file and directory path selection control with a built-in file dialog.
/// Supports file filtering, multi-selection, adjacent-path navigation, validation, and displaying file sizes.
/// </summary>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;se:StswPathPicker SelectedPath="{Binding FilePath}" SelectionUnit="OpenFile" Filter="Text Files|*.txt"/&gt;
/// </code>
/// </example>
[ContentProperty(nameof(SelectedPath))]
public class StswPathPicker : StswInputBoxBase, IStswBoxControl, IStswCornerControl
{
    static StswPathPicker()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswPathPicker), new FrameworkPropertyMetadata(typeof(StswPathPicker)));
    }

    public StswPathPicker()
    {
        SetValue(SubControlsProperty, new ObservableCollection<IStswSubControl>());
    }

    #region Dependency properties

    /// <inheritdoc/>
    public bool CornerClipping
    {
        get => (bool)GetValue(CornerClippingProperty);
        set => SetValue(CornerClippingProperty, value);
    }
    public static readonly DependencyProperty CornerClippingProperty =
        DependencyProperty.Register(nameof(CornerClipping), typeof(bool), typeof(StswPathPicker));

    /// <inheritdoc/>
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }
    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(StswPathPicker));

    /// <inheritdoc/>
    public ReadOnlyObservableCollection<ValidationError> Errors
    {
        get => (ReadOnlyObservableCollection<ValidationError>)GetValue(ErrorsProperty);
        set => SetValue(ErrorsProperty, value);
    }
    public static readonly DependencyProperty ErrorsProperty =
        DependencyProperty.Register(nameof(Errors), typeof(ReadOnlyObservableCollection<ValidationError>), typeof(StswPathPicker));

    /// <summary>
    /// Gets the associated icon for the selected file or directory.
    /// </summary>
    public ImageSource? FileIcon
    {
        get => (ImageSource?)GetValue(FileIconProperty);
        private set => SetValue(FileIconProperty, value);
    }
    public static readonly DependencyProperty FileIconProperty =
        DependencyProperty.Register(nameof(FileIcon), typeof(ImageSource), typeof(StswPathPicker));

    /// <summary>
    /// Gets the formatted size of the selected file, when available.
    /// </summary>
    internal string? FileSize
    {
        get => (string?)GetValue(FileSizeProperty);
        private set => SetValue(FileSizeProperty, value);
    }
    internal static readonly DependencyProperty FileSizeProperty =
        DependencyProperty.Register(nameof(FileSize), typeof(string), typeof(StswPathPicker));

    /// <summary>
    /// Gets or sets the file filter used in the file selection dialog.
    /// Example: "Image Files (*.png;*.jpg)|*.png;*.jpg".
    /// </summary>
    public string Filter
    {
        get => (string)GetValue(FilterProperty);
        set => SetValue(FilterProperty, value);
    }
    public static readonly DependencyProperty FilterProperty =
        DependencyProperty.Register(
            nameof(Filter),
            typeof(string),
            typeof(StswPathPicker),
            new PropertyMetadata(default(string), OnFilterChanged));

    private static void OnFilterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var input = (StswPathPicker)d;
        if (input.SelectionUnit != StswPathType.OpenDirectory)
            input.ListAdjacentPaths();
    }

    /// <inheritdoc/>
    public bool HasError
    {
        get => (bool)GetValue(HasErrorProperty);
        set => SetValue(HasErrorProperty, value);
    }
    public static readonly DependencyProperty HasErrorProperty =
        DependencyProperty.Register(nameof(HasError), typeof(bool), typeof(StswPathPicker));

    /// <inheritdoc/>
    public object? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }
    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(nameof(Icon), typeof(object), typeof(StswPathPicker));

    /// <summary>
    /// Gets or sets whether the selected file size is displayed.
    /// </summary>
    public bool IsFileSizeVisible
    {
        get => (bool)GetValue(IsFileSizeVisibleProperty);
        set => SetValue(IsFileSizeVisibleProperty, value);
    }
    public static readonly DependencyProperty IsFileSizeVisibleProperty =
        DependencyProperty.Register(nameof(IsFileSizeVisible), typeof(bool), typeof(StswPathPicker));

    /// <summary>
    /// Gets or sets a value indicating whether mouse-wheel navigation through adjacent paths is enabled.
    /// </summary>
    public bool IsShiftingEnabled
    {
        get => (bool)GetValue(IsShiftingEnabledProperty);
        set => SetValue(IsShiftingEnabledProperty, value);
    }
    public static readonly DependencyProperty IsShiftingEnabledProperty =
        DependencyProperty.Register(
            nameof(IsShiftingEnabled),
            typeof(bool),
            typeof(StswPathPicker),
            new FrameworkPropertyMetadata(
                default(bool),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnIsShiftingEnabledChanged,
                null,
                false,
                UpdateSourceTrigger.PropertyChanged));

    public static void OnIsShiftingEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        => ((StswPathPicker)d).ListAdjacentPaths();

    /// <summary>
    /// Gets or sets whether the open-file dialog allows selecting multiple files.
    /// </summary>
    public bool Multiselect
    {
        get => (bool)GetValue(MultiselectProperty);
        set => SetValue(MultiselectProperty, value);
    }
    public static readonly DependencyProperty MultiselectProperty =
        DependencyProperty.Register(nameof(Multiselect), typeof(bool), typeof(StswPathPicker));

    /// <summary>
    /// Gets or sets the currently selected path.
    /// </summary>
    public string? SelectedPath
    {
        get => (string?)GetValue(SelectedPathProperty);
        set => SetValue(SelectedPathProperty, value);
    }
    public static readonly DependencyProperty SelectedPathProperty =
        DependencyProperty.Register(
            nameof(SelectedPath),
            typeof(string),
            typeof(StswPathPicker),
            new FrameworkPropertyMetadata(
                default(string?),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnSelectedPathChanged,
                null,
                false,
                UpdateSourceTrigger.PropertyChanged));

    public static void OnSelectedPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var input = (StswPathPicker)d;

        input.FileSize = File.Exists(input.SelectedPath)
            ? StswFn.FormatByteSize(new FileInfo(input.SelectedPath).Length)
            : null;
        input.FileIcon = StswFnUI.ExtractAssociatedIcon(input.SelectedPath)?.ToImageSource();

        if (Path.Exists(input.SelectedPath)
            && Directory.GetParent(input.SelectedPath!)?.FullName is string parentPath
            && parentPath != input._parentPath)
        {
            input._parentPath = parentPath;
            input.ListAdjacentPaths();
        }
    }

    private string? _parentPath;

    /// <summary>
    /// Gets or sets all paths selected by a multiselect dialog.
    /// </summary>
    public string[] SelectedPaths
    {
        get => (string[])GetValue(SelectedPathsProperty);
        set => SetValue(SelectedPathsProperty, value);
    }
    public static readonly DependencyProperty SelectedPathsProperty =
        DependencyProperty.Register(
            nameof(SelectedPaths),
            typeof(string[]),
            typeof(StswPathPicker),
            new PropertyMetadata(Array.Empty<string>()));

    /// <summary>
    /// Gets or sets the type of path selected by the control.
    /// </summary>
    public StswPathType SelectionUnit
    {
        get => (StswPathType)GetValue(SelectionUnitProperty);
        set => SetValue(SelectionUnitProperty, value);
    }
    public static readonly DependencyProperty SelectionUnitProperty =
        DependencyProperty.Register(
            nameof(SelectionUnit),
            typeof(StswPathType),
            typeof(StswPathPicker),
            new PropertyMetadata(default(StswPathType), OnIsShiftingEnabledChanged));

    /// <summary>
    /// Gets or sets the thickness of separators inside the control.
    /// </summary>
    public double SeparatorThickness
    {
        get => (double)GetValue(SeparatorThicknessProperty);
        set => SetValue(SeparatorThicknessProperty, value);
    }
    public static readonly DependencyProperty SeparatorThicknessProperty =
        DependencyProperty.Register(
            nameof(SeparatorThickness),
            typeof(double),
            typeof(StswPathPicker),
            new FrameworkPropertyMetadata(default(double), FrameworkPropertyMetadataOptions.AffectsRender));

    /// <inheritdoc/>
    public ObservableCollection<IStswSubControl> SubControls
    {
        get => (ObservableCollection<IStswSubControl>)GetValue(SubControlsProperty);
        set => SetValue(SubControlsProperty, value);
    }
    public static readonly DependencyProperty SubControlsProperty =
        DependencyProperty.Register(nameof(SubControls), typeof(ObservableCollection<IStswSubControl>), typeof(StswPathPicker));

    /// <summary>
    /// Gets or sets the suggested file name shown by file dialogs.
    /// </summary>
    public string? SuggestedFilename
    {
        get => (string?)GetValue(SuggestedFilenameProperty);
        set => SetValue(SuggestedFilenameProperty, value);
    }
    public static readonly DependencyProperty SuggestedFilenameProperty =
        DependencyProperty.Register(nameof(SuggestedFilename), typeof(string), typeof(StswPathPicker));

    /// <summary>
    /// Internal editing buffer. <see cref="SelectedPath"/> is the public semantic value of the control.
    /// </summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public new string? Text
    {
        get => base.Text;
        internal set => base.Text = value ?? string.Empty;
    }

    #endregion

    #region Template and overrides

    private ButtonBase? _dialogButton;

    /// <inheritdoc/>
    public override void OnApplyTemplate()
    {
        DetachTemplateEvents();
        base.OnApplyTemplate();

        _dialogButton = GetTemplateChild("PART_DialogButton") as ButtonBase;
        AttachTemplateEvents();

        AttachTextValidationRule();
        ListAdjacentPaths();
    }

    private void AttachTemplateEvents()
    {
        if (_dialogButton is not null)
            _dialogButton.Click += PART_DialogButton_Click;
    }

    private void DetachTemplateEvents()
    {
        if (_dialogButton is not null)
            _dialogButton.Click -= PART_DialogButton_Click;

        _dialogButton = null;
    }

    /// <summary>
    /// Enter is the explicit path commit path supplied by <see cref="StswInputBoxBase.CommitOnEnter"/>.
    /// </summary>
    protected override void OnCommit()
    {
        UpdateMainProperty(alwaysUpdate: true);
    }

    /// <summary>
    /// Losing focus commits a valid changed path.
    /// </summary>
    protected override void OnLostFocus(RoutedEventArgs e)
    {
        UpdateMainProperty(alwaysUpdate: false);
        base.OnLostFocus(e);
    }

    /// <inheritdoc/>
    protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
    {
        if (IsKeyboardFocused && !IsReadOnly && IsShiftingEnabled)
        {
            ShiftBy(e.Delta > 0 ? -1 : 1);
            e.Handled = true;
            return;
        }

        base.OnPreviewMouseWheel(e);
    }

    #endregion

    #region Logic

    /// <summary>
    /// Rebuilds the Text binding with the path validation rule used by the historical control.
    /// Text remains an editing buffer; updates are explicitly committed by this control.
    /// </summary>
    private void AttachTextValidationRule()
    {
        var current = BindingOperations.GetBinding(this, TextProperty);

        var binding = new Binding
        {
            Path = current?.Path ?? new PropertyPath(nameof(SelectedPath)),
            Mode = current?.Mode ?? BindingMode.TwoWay,
            RelativeSource = current?.RelativeSource ?? new RelativeSource(RelativeSourceMode.Self),
            TargetNullValue = current?.TargetNullValue ?? string.Empty,
            StringFormat = current?.StringFormat,
            Converter = current?.Converter,
            ConverterCulture = current?.ConverterCulture,
            ConverterParameter = current?.ConverterParameter,
            UpdateSourceTrigger = UpdateSourceTrigger.Explicit
        };
        binding.ValidationRules.Add(new StswPathExistsValidationRule { Host = this });

        BindingOperations.SetBinding(this, TextProperty, binding);
    }

    /// <summary>
    /// Lists adjacent paths based on the current selected path and path type.
    /// </summary>
    private void ListAdjacentPaths()
    {
        if (!IsShiftingEnabled || _parentPath is null)
        {
            _adjacentPaths = null;
            return;
        }

        try
        {
            if (SelectionUnit == StswPathType.OpenDirectory)
            {
                _adjacentPaths = [.. Directory.GetDirectories(_parentPath)];
            }
            else
            {
                var all = Directory.GetFiles(_parentPath);
                _adjacentPaths = [.. all.Where(path => StswPathFilterHelper.IsFileAllowed(path, Filter))];
            }
        }
        catch
        {
            _adjacentPaths = null;
        }
    }

    private IList<string>? _adjacentPaths;

    /// <summary>
    /// Shifts the selected path by a specified step, updating it with an adjacent path.
    /// </summary>
    private void ShiftBy(int step)
    {
        if (string.IsNullOrEmpty(SelectedPath) || _adjacentPaths?.Count <= 0)
            return;

        var currentIndex = _adjacentPaths.IndexOf(SelectedPath);
        var nextIndex = currentIndex + step;

        if (nextIndex.Between(0, _adjacentPaths.Count - 1))
            SelectedPath = _adjacentPaths[nextIndex];
        else if (nextIndex < 0)
            SelectedPath = _adjacentPaths[0];
        else
            SelectedPath = _adjacentPaths[^1];
    }

    /// <summary>
    /// Parses and validates the editing buffer and commits <see cref="SelectedPath"/>.
    /// </summary>
    private void UpdateMainProperty(bool alwaysUpdate)
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
                        else
                        {
                            isInvalid = true;
                        }
                        break;

                    case StswPathType.OpenFile:
                        if (File.Exists(normalized))
                        {
                            isPlain = true;
                            nextSelectedPath = normalized;
                        }
                        else
                        {
                            isInvalid = true;
                        }
                        break;

                    case StswPathType.SaveFile:
                        var directory = Path.GetDirectoryName(normalized);
                        var fileName = Path.GetFileName(normalized);

                        var directoryOk = !string.IsNullOrEmpty(directory) && Directory.Exists(directory);
                        var fileNameOk = !string.IsNullOrWhiteSpace(fileName)
                            && fileName.IndexOfAny(Path.GetInvalidFileNameChars()) < 0;

                        if (directoryOk && fileNameOk)
                        {
                            isPlain = true;
                            nextSelectedPath = normalized;
                        }
                        else
                        {
                            isInvalid = true;
                        }
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

        if (Equals(nextSelectedPath, SelectedPath) && !alwaysUpdate)
            return;

        SelectedPath = nextSelectedPath;

        var valueBinding = GetBindingExpression(SelectedPathProperty);
        if (!isInvalid && valueBinding?.Status == BindingStatus.Active)
            valueBinding.UpdateSource();

        if (nextSelectedPath is null)
            SelectedPaths = [];
        else if (SelectionUnit != StswPathType.OpenDirectory)
            SelectedPaths = [nextSelectedPath];

        var textBinding = GetBindingExpression(TextProperty);
        if (textBinding != null && textBinding.Status is BindingStatus.Active or BindingStatus.UpdateSourceError)
        {
            if (string.IsNullOrWhiteSpace(Text) || isPlain)
                textBinding.UpdateSource();
            else if (isInvalid && alwaysUpdate)
                textBinding.UpdateSource();
        }
    }

    /// <summary>
    /// Opens a directory or file selection dialog according to <see cref="SelectionUnit"/>.
    /// </summary>
    private void PART_DialogButton_Click(object sender, RoutedEventArgs e)
    {
        if (SelectionUnit == StswPathType.OpenDirectory)
        {
            using var folderDialog = new System.Windows.Forms.FolderBrowserDialog();
            if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                SelectedPath = folderDialog.SelectedPath;
                SelectedPaths = [folderDialog.SelectedPath];
            }
            return;
        }

        using System.Windows.Forms.FileDialog dialog = SelectionUnit switch
        {
            StswPathType.OpenFile => new System.Windows.Forms.OpenFileDialog(),
            StswPathType.SaveFile => new System.Windows.Forms.SaveFileDialog(),
            _ => throw new NotImplementedException()
        };

        try
        {
            dialog.Filter = Filter;
        }
        catch
        {
            // Preserve historical behaviour: an invalid filter must not prevent opening the dialog.
        }

        if (dialog is System.Windows.Forms.OpenFileDialog openFileDialog)
            openFileDialog.Multiselect = Multiselect;

        if (SuggestedFilename is not null)
            dialog.FileName = SuggestedFilename;

        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            SelectedPath = dialog.FileName;
            SelectedPaths = dialog.FileNames;
        }
    }

    #endregion
}
