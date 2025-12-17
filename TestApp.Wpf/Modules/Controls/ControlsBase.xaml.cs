using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Xml.Linq;

namespace TestApp.Wpf;
/// <summary>
/// Interaction logic for ControlsBase.xaml
/// </summary>
public partial class ControlsBase : UserControl
{
    private readonly Stopwatch _loadingStopwatch = Stopwatch.StartNew();
    private static readonly Dictionary<Assembly, XDocument?> _xmlDocumentationCache = [];
    private static readonly Dictionary<string, string?> _summaryCache = [];

    public ControlsBase()
    {
        InitializeComponent();
        SetValue(PropertiesProperty, new ObservableCollection<UIElement>());

        DataContextChanged += ControlsBase_DataContextChanged;
        Loaded += ControlsBase_Loaded;
    }

    #region Events & methods
    /// <summary>
    /// Handles the DataContextChanged event to update the description from XML documentation.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    private void ControlsBase_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) => TrySetDescriptionFromSummary();

    /// <summary>
    /// Handles the Loaded event to measure loading time and update the description.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    private async void ControlsBase_Loaded(object sender, RoutedEventArgs e)
    {
        Loaded -= ControlsBase_Loaded;

        await Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ContextIdle);

        if (_loadingStopwatch.IsRunning)
        {
            _loadingStopwatch.Stop();
            LoadingTime = $"{_loadingStopwatch.Elapsed.TotalMilliseconds:F0} ms";
        }

        TrySetDescriptionFromSummary();
    }

    /// <summary>
    /// Attempts to set the Description property from the XML documentation summary of the control type.
    /// </summary>
    private void TrySetDescriptionFromSummary()
    {
        var controlName = DataContext?.GetType().GetProperty("ThisControlName")?.GetValue(DataContext) as string;
        if (string.IsNullOrWhiteSpace(controlName))
            return;

        var controlType = Type.GetType($"StswExpress.Wpf.{controlName}, StswExpress.Wpf");
        if (controlType == null)
            return;

        var summary = GetSummary(controlType);
        if (!string.IsNullOrWhiteSpace(summary))
            Description = summary;

        IsContentAlignmentVisible = HasContentAlignment(controlType);
    }

    /// <summary>
    /// Retrieves the XML documentation summary for the specified type.
    /// </summary>
    /// <param name="type">The type to retrieve the summary for.</param>
    /// <returns>The summary text, or null if not found.</returns>
    private static string? GetSummary(Type type)
    {
        var cacheKey = $"{type.Assembly.FullName}:{type.FullName}";
        if (_summaryCache.TryGetValue(cacheKey, out var cachedSummary))
            return cachedSummary;

        var documentation = GetXmlDocumentation(type.Assembly);
        if (documentation != null)
        {
            var memberName = $"T:{type.FullName}";
            var summary = documentation.Descendants("member")
                .FirstOrDefault(x => x.Attribute("name")?.Value == memberName)?
                .Descendants("summary")
                .FirstOrDefault()?
                .Value
                .Trim();

            _summaryCache[cacheKey] = string.IsNullOrWhiteSpace(summary)
                ? null
                : StswFn.RemoveConsecutiveText(summary, "  ");
        }
        else
        {
            _summaryCache[cacheKey] = null;
        }

        return _summaryCache[cacheKey];
    }

    /// <summary>
    /// Retrieves the XML documentation for the specified assembly.
    /// </summary>
    /// <param name="assembly">The assembly to retrieve the documentation for.</param>
    /// <returns>The XML documentation as an XDocument, or null if not found.</returns>
    private static XDocument? GetXmlDocumentation(Assembly assembly)
    {
        if (_xmlDocumentationCache.TryGetValue(assembly, out var documentation))
            return documentation;

        var documentationPath = Path.ChangeExtension(assembly.Location, ".xml");
        if (documentationPath != null && File.Exists(documentationPath))
        {
            try
            {
                documentation = XDocument.Load(documentationPath);
            }
            catch
            {
                documentation = null;
            }
        }

        _xmlDocumentationCache[assembly] = documentation;
        return documentation;
    }

    /// <summary>
    /// Determines whether the specified control type has content alignment properties.
    /// </summary>
    /// <param name="controlType">The control type to check.</param>
    /// <returns><see langword="true"/> if the control type has content alignment properties; otherwise, <see langword="false"/>.</returns>
    private static bool HasContentAlignment(Type controlType)
    {
        static bool HasDependencyProperty(Type type, string fieldName)
            => type.GetField(fieldName, BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy) != null;

        return HasDependencyProperty(controlType, nameof(Control.HorizontalContentAlignmentProperty))
            && HasDependencyProperty(controlType, nameof(Control.VerticalContentAlignmentProperty));
    }
    #endregion

    #region Logic properties
    /// <summary>
    /// Gets or sets the description text.
    /// </summary>
    public string? Description
    {
        get => (string?)GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }
    public static readonly DependencyProperty DescriptionProperty
        = DependencyProperty.Register(
            nameof(Description),
            typeof(string),
            typeof(ControlsBase)
        );

    /// <summary>
    /// Gets or sets a value indicating whether the content alignment options are visible.
    /// </summary>
    public bool IsContentAlignmentVisible
    {
        get => (bool)GetValue(IsContentAlignmentVisibleProperty);
        set => SetValue(IsContentAlignmentVisibleProperty, value);
    }
    public static readonly DependencyProperty IsContentAlignmentVisibleProperty
        = DependencyProperty.Register(
            nameof(IsContentAlignmentVisible),
            typeof(bool),
            typeof(ControlsBase)
        );

    /// <summary>
    /// Gets or sets the loading time text displayed beneath the description.
    /// </summary>
    public string? LoadingTime
    {
        get => (string?)GetValue(LoadingTimeProperty);
        set => SetValue(LoadingTimeProperty, value);
    }
    public static readonly DependencyProperty LoadingTimeProperty
        = DependencyProperty.Register(
            nameof(LoadingTime),
            typeof(string),
            typeof(ControlsBase)
        );

    /// <summary>
    /// Gets or sets the collection of property elements.
    /// </summary>
    public ObservableCollection<UIElement> Properties
    {
        get => (ObservableCollection<UIElement>)GetValue(PropertiesProperty);
        set => SetValue(PropertiesProperty, value);
    }
    public static readonly DependencyProperty PropertiesProperty
        = DependencyProperty.Register(
            nameof(Properties),
            typeof(ObservableCollection<UIElement>),
            typeof(ControlsBase)
        );

    /// <summary>
    /// Gets or sets the status panel element.
    /// </summary>
    public UIElement StatusPanel
    {
        get => (UIElement)GetValue(StatusPanelProperty);
        set => SetValue(StatusPanelProperty, value);
    }
    public static readonly DependencyProperty StatusPanelProperty
        = DependencyProperty.Register(
            nameof(StatusPanel),
            typeof(UIElement),
            typeof(ControlsBase)
        );
    #endregion
}
