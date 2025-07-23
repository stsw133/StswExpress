using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace StswExpress;

/// <summary>
/// 
/// </summary>
[StswInfo("0.19.0", Changes = StswPlannedChanges.Finish)]
public class StswBetterNavigation : ItemsControl
{
    static StswBetterNavigation()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswBetterNavigation), new FrameworkPropertyMetadata(typeof(StswBetterNavigation)));
        //ToolTipService.ToolTipProperty.OverrideMetadata(typeof(StswStepBar), new FrameworkPropertyMetadata(null, StswToolTip.OnToolTipChanged));
    }
    public StswBetterNavigation()
    {
        // Constructor logic can be added here if needed
    }

    #region Logic Properties
    public new IEnumerable<IStswBetterNavigationItem> ItemsSource
    {
        get => (IEnumerable<IStswBetterNavigationItem>)base.ItemsSource;
        set => base.ItemsSource = value;
    }
    #endregion
}

/// <summary>
/// 
/// </summary>
[StswInfo("0.19.0", Changes = StswPlannedChanges.Finish)]
public class StswBetterNavigationGroup : ItemsControl, IStswBetterNavigationItem
{
    public bool IsGroupElement => true;

    public StswBetterNavigationGroup()
    {
    }

    #region Logic Properties
    public new IEnumerable<IStswBetterNavigationItem> ItemsSource
    {
        get => (IEnumerable<IStswBetterNavigationItem>)base.ItemsSource;
        set => base.ItemsSource = value;
    }
    #endregion
}

/// <summary>
/// 
/// </summary>
[StswInfo("0.19.0", Changes = StswPlannedChanges.Finish)]
public class StswBetterNavigationElement : ContentControl ,IStswBetterNavigationItem
{
    public bool IsGroupElement => false;
}

/// <summary>
/// 
/// </summary>
[StswInfo("0.19.0", Changes = StswPlannedChanges.Finish)]
public interface IStswBetterNavigationItem
{
    public bool IsGroupElement { get; }
}