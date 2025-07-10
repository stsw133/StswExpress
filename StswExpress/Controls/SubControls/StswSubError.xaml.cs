using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace StswExpress;

/// <summary>
/// A sub-control that displays an error icon when validation errors occur.
/// The tooltip provides additional error details.
/// </summary>
/// <remarks>
/// This control is designed to be used inside input fields or forms to indicate validation issues.
/// It automatically updates visibility and tooltip content based on validation errors.
/// </remarks>
[Browsable(false)]
[EditorBrowsable(EditorBrowsableState.Never)]
[StswInfo("0.6.1")]
public class StswSubError : StswSubLabel
{
    static StswSubError()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswSubError), new FrameworkPropertyMetadata(typeof(StswSubError)));
    }

    #region Logic properties
    /// <summary>
    /// Gets or sets a read-only collection of validation errors displayed in the control's tooltip.
    /// </summary>
    public ReadOnlyObservableCollection<ValidationError> Errors
    {
        get => (ReadOnlyObservableCollection<ValidationError>)GetValue(ErrorsProperty);
        set => SetValue(ErrorsProperty, value);
    }
    public static readonly DependencyProperty ErrorsProperty
        = DependencyProperty.Register(
            nameof(Errors),
            typeof(ReadOnlyObservableCollection<ValidationError>),
            typeof(StswSubError)
        );

    /// <summary>
    /// Gets or sets a value indicating whether the error indicator is visible.
    /// When set to <see langword="true"/>, the error icon is displayed inside the control.
    /// </summary>
    public bool HasError
    {
        get => (bool)GetValue(HasErrorProperty);
        set => SetValue(HasErrorProperty, value);
    }
    public static readonly DependencyProperty HasErrorProperty
        = DependencyProperty.Register(
            nameof(HasError),
            typeof(bool),
            typeof(StswSubError)
        );
    #endregion
}

/* usage:

<se:StswSubError HasError="True"/>

*/
