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
        if (GetTemplateChild("PART_FunctionButton") is Button btn)
            btn.Click += PART_FunctionButton_Click;

        base.OnApplyTemplate();
    }

    /// PART_FunctionButton_Click
    public void PART_FunctionButton_Click(object sender, RoutedEventArgs e)
    {
        var newTab = new StswTabItem()
        {
            Header = new StswHeader()
            {
                IconData = NewTabTemplate.Icon,
                Content = NewTabTemplate.Name
            },
            Closable = true,
            Content = Activator.CreateInstance(NewTabTemplate.Type)
        };

        int? index;
        if (ItemsSource != null)
            index = (ItemsSource as IList)?.Add(newTab);
        else
            index = Items?.Add(newTab);
        SelectedIndex = index ?? -1;
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

    /// SpecialButtonVisibility
    public static readonly DependencyProperty SpecialButtonVisibilityProperty
        = DependencyProperty.Register(
            nameof(SpecialButtonVisibility),
            typeof(Visibility),
            typeof(StswTabControl)
        );
    public Visibility SpecialButtonVisibility
    {
        get => (Visibility)GetValue(SpecialButtonVisibilityProperty);
        set => SetValue(SpecialButtonVisibilityProperty, value);
    }
    #endregion
}

public class StswTabItemModel
{
    public Geometry? Icon { get; set; }
    public string? Name { get; set; }
    public Type Type { get; set; }
}