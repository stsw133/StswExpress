﻿using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace StswExpress;
/// <summary>
/// Represents a control that extends the <see cref="TabControl"/> class with additional functionality.
/// </summary>
public class StswTabControl : TabControl
{
    static StswTabControl()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswTabControl), new FrameworkPropertyMetadata(typeof(StswTabControl)));
    }

    #region Events & methods
    /// <summary>
    /// Occurs when the template is applied to the control.
    /// </summary>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        /// NewTabButton
        if (GetTemplateChild("PART_NewTabButton") is ButtonBase newTabButton)
            newTabButton.Click += PART_NewTabButton_Click;
    }

    /// <summary>
    /// Handles the click event of the PART_NewTabButton.
    /// Creates and adds a new tab item based on the NewTabTemplate.
    /// </summary>
    /// <param name="sender">The sender object triggering the event</param>
    /// <param name="e">The event arguments</param>
    public void PART_NewTabButton_Click(object sender, RoutedEventArgs e)
    {
        if (NewTabTemplate == null)
            return;

        var newTab = new StswTabItem()
        {
            Header = new StswLabel()
            {
                Content = NewTabTemplate.Name,
                IconData = NewTabTemplate.Icon,
                HorizontalContentAlignment = HorizontalAlignment.Left
            },
            IsClosable = true,
            Content = NewTabTemplate.Type != null ? Activator.CreateInstance(NewTabTemplate.Type) : null
        };

        int index = -1;
        if (ItemsSource is IList list)
            index = list.Add(newTab);
        else if (Items != null)
            index = Items.Add(newTab);
        SelectedIndex = index;
    }
    #endregion

    #region Logic properties
    /// <summary>
    /// Gets or sets a value indicating whether the tabs are visible in the tab control.
    /// </summary>
    public bool AreTabsVisible
    {
        get => (bool)GetValue(AreTabsVisibleProperty);
        set => SetValue(AreTabsVisibleProperty, value);
    }
    public static readonly DependencyProperty AreTabsVisibleProperty
        = DependencyProperty.Register(
            nameof(AreTabsVisible),
            typeof(bool),
            typeof(StswTabControl)
        );

    /// <summary>
    /// Gets or sets the visibility of the new tab button.
    /// </summary>
    public Visibility NewTabButtonVisibility
    {
        get => (Visibility)GetValue(NewTabButtonVisibilityProperty);
        set => SetValue(NewTabButtonVisibilityProperty, value);
    }
    public static readonly DependencyProperty NewTabButtonVisibilityProperty
        = DependencyProperty.Register(
            nameof(NewTabButtonVisibility),
            typeof(Visibility),
            typeof(StswTabControl)
        );

    /// <summary>
    /// Gets or sets the template for creating new tab items in the tab control.
    /// </summary>
    public StswTabItemModel? NewTabTemplate
    {
        get => (StswTabItemModel?)GetValue(NewTabTemplateProperty);
        set => SetValue(NewTabTemplateProperty, value);
    }
    public static readonly DependencyProperty NewTabTemplateProperty
        = DependencyProperty.Register(
            nameof(NewTabTemplate),
            typeof(StswTabItemModel),
            typeof(StswTabControl)
        );
    #endregion
}

/// <summary>
/// Data model for StswTabControl's new tab template.
/// </summary>
public class StswTabItemModel
{
    /// <summary>
    /// Gets or sets the icon data represented by a Geometry object for the new tab item.
    /// </summary>
    public Geometry? Icon { get; set; }

    /// <summary>
    /// Gets or sets the name for the new tab item.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the type of the content to be created for the new tab item.
    /// </summary>
    public Type? Type { get; set; }
}
