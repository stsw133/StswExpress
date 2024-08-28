using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace StswExpress;

/// <summary>
/// Represents a sub control displaying error icon.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public class StswSubError : StswSubLabel
{
    static StswSubError()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswSubError), new FrameworkPropertyMetadata(typeof(StswSubError)));
    }

    #region Logic properties
    /// <summary>
    /// 
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
    /// 
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
