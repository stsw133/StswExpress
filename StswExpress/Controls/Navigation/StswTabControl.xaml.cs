using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace StswExpress;

public class StswTabControl : TabControl
{
    static StswTabControl()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswTabControl), new FrameworkPropertyMetadata(typeof(StswTabControl)));
    }

    #region Events
    /// OnApplyTemplate
    public override void OnApplyTemplate()
    {
        /// FunctionButton
        if (GetTemplateChild("PART_FunctionButton") is StswButton btn)
            btn.Click += PART_FunctionButton_Click;

        base.OnApplyTemplate();
    }

    /// PART_FunctionButton_Click
    public void PART_FunctionButton_Click(object sender, RoutedEventArgs e)
    {
        if (NewTabTemplate == null)
            return;

        var newTab = new StswTabItem()
        {
            Header = new StswHeader()
            {
                IconData = NewTabTemplate.Icon,
                Content = NewTabTemplate.Name
            },
            IsClosable = true,
            Content = NewTabTemplate.Type != null ? Activator.CreateInstance(NewTabTemplate.Type) : null
        };

        int index = -1;
        if (ItemsSource is IList list and not null)
            index = list.Add(newTab);
        else if (Items != null)
            index = Items.Add(newTab);
        SelectedIndex = index;
    }
    #endregion

    #region Main properties
    /// AreTabsVisible
    public static readonly DependencyProperty AreTabsVisibleProperty
        = DependencyProperty.Register(
            nameof(AreTabsVisible),
            typeof(bool),
            typeof(StswTabControl)
        );
    public bool AreTabsVisible
    {
        get => (bool)GetValue(AreTabsVisibleProperty);
        set => SetValue(AreTabsVisibleProperty, value);
    }

    /// NewTabButtonVisibility
    public static readonly DependencyProperty NewTabButtonVisibilityProperty
        = DependencyProperty.Register(
            nameof(NewTabButtonVisibility),
            typeof(Visibility),
            typeof(StswTabControl)
        );
    public Visibility NewTabButtonVisibility
    {
        get => (Visibility)GetValue(NewTabButtonVisibilityProperty);
        set => SetValue(NewTabButtonVisibilityProperty, value);
    }
    /// NewTabTemplate
    public static readonly DependencyProperty NewTabTemplateProperty
        = DependencyProperty.Register(
            nameof(NewTabTemplate),
            typeof(StswTabItemModel),
            typeof(StswTabControl)
        );
    public StswTabItemModel NewTabTemplate
    {
        get => (StswTabItemModel)GetValue(NewTabTemplateProperty);
        set => SetValue(NewTabTemplateProperty, value);
    }
    #endregion
}

public class StswTabItemModel
{
    public Geometry? Icon { get; set; }
    public string? Name { get; set; }
    public Type? Type { get; set; }
}