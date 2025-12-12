using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.Collections.ObjectModel;

namespace TestApp.Ava;
/// <summary>
/// Interaction logic for ControlsBase.xaml
/// </summary>
public partial class ControlsBase : UserControl
{
    public ControlsBase()
    {
        AvaloniaXamlLoader.Load(this);
        //SetValue(PropertiesProperty, new ObservableCollection<Layoutable>());
    }
    /*
    #region Logic properties
    /// <summary>
    /// 
    /// </summary>
    public string? Description
    {
        get => (string?)GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }
    public static readonly AvaloniaProperty DescriptionProperty = AvaloniaProperty.Register<ControlsBase, string?>(nameof(Description));

    /// <summary>
    /// 
    /// </summary>
    public bool IsContentAlignmentVisibible
    {
        get => (bool)(GetValue(IsContentAlignmentVisibibleProperty) ?? false);
        set => SetValue(IsContentAlignmentVisibibleProperty, value);
    }
    public static readonly AvaloniaProperty IsContentAlignmentVisibibleProperty = AvaloniaProperty.Register<ControlsBase, bool>(nameof(IsContentAlignmentVisibible));

    /// <summary>
    /// 
    /// </summary>
    public ObservableCollection<Layoutable>? Properties
    {
        get => (ObservableCollection<Layoutable>?)GetValue(PropertiesProperty);
        set => SetValue(PropertiesProperty, value);
    }
    public static readonly AvaloniaProperty PropertiesProperty = AvaloniaProperty.Register<ControlsBase, ObservableCollection<Layoutable>?>(nameof(Properties));

    /// <summary>
    /// 
    /// </summary>
    public Layoutable? StatusPanel
    {
        get => (Layoutable?)GetValue(StatusPanelProperty);
        set => SetValue(StatusPanelProperty, value);
    }
    public static readonly AvaloniaProperty StatusPanelProperty = AvaloniaProperty.Register<ControlsBase, Layoutable?>(nameof(StatusPanel));
    #endregion
    */
}
