using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Markup;

namespace StswExpress;

/// <summary>
/// Represents a control that allows shifting through items by clicking button.
/// </summary>
public class StswShiftSwitch : ComboBox
{
    static StswShiftSwitch()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswShiftSwitch), new FrameworkPropertyMetadata(typeof(StswShiftSwitch)));
    }

    #region Events & methods
    /// <summary>
    /// Occurs when the template is applied to the control.
    /// </summary>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        /// Button: next
        if (GetTemplateChild("PART_MainButton") is ButtonBase btnNext)
            btnNext.Click += (s, e) => SelectedIndex = (SelectedIndex + 1) % Items.Count;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="e"></param>
    protected override void OnSelectionChanged(SelectionChangedEventArgs e)
    {
        base.OnSelectionChanged(e);
        IsChecked = SelectedIndex == 1 ? true : SelectedIndex == 0 ? false : null;
    }
    #endregion

    #region Main properties
    /// <summary>
    /// 
    /// </summary>
    public bool? IsChecked
    {
        get => (bool?)GetValue(IsCheckedProperty);
        set => SetValue(IsCheckedProperty, value);
    }
    public static readonly DependencyProperty IsCheckedProperty
        = DependencyProperty.Register(
            nameof(IsChecked),
            typeof(bool?),
            typeof(StswShiftSwitch),
            new FrameworkPropertyMetadata(default(bool?),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnIsCheckedChanged, null, false, UpdateSourceTrigger.PropertyChanged)
        );
    public static void OnIsCheckedChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is StswShiftSwitch stsw)
        {
            stsw.SelectedIndex = Math.Min(stsw.IsChecked == true ? 1 : stsw.IsChecked == false ? 0 : 2, stsw.Items.Count - 1);
        }
    }
    #endregion
}
