using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;

namespace StswExpress.Wpf;

/// <summary>
/// A standard text input control for displaying or editing unformatted text.
/// Supports placeholder text, validation, and an optional icon.
/// </summary>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;se:StswInputBoxTest Text="{Binding Email}" Icon="{StaticResource MailIcon}" Placeholder="Enter your email"/&gt;
/// </code>
/// </example>
[ContentProperty(nameof(Text))]
public class StswInputBoxTest : StswInputBoxBase, IStswBoxControl, IStswCornerControl
{
    static StswInputBoxTest()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StswInputBoxTest), new FrameworkPropertyMetadata(typeof(StswInputBoxTest)));
    }
    public StswInputBoxTest()
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
    public static readonly DependencyProperty CornerClippingProperty
        = DependencyProperty.Register(
            nameof(CornerClipping),
            typeof(bool),
            typeof(StswInputBoxTest)
        );

    /// <inheritdoc/>
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }
    public static readonly DependencyProperty CornerRadiusProperty
        = DependencyProperty.Register(
            nameof(CornerRadius),
            typeof(CornerRadius),
            typeof(StswInputBoxTest)
        );

    /// <inheritdoc/>
    public ReadOnlyObservableCollection<ValidationError> Errors
    {
        get => (ReadOnlyObservableCollection<ValidationError>)GetValue(ErrorsProperty);
        set => SetValue(ErrorsProperty, value);
    }
    public static readonly DependencyProperty ErrorsProperty
        = DependencyProperty.Register(
            nameof(Errors),
            typeof(ReadOnlyObservableCollection<ValidationError>),
            typeof(StswInputBoxTest)
        );

    /// <inheritdoc/>
    public bool HasError
    {
        get => (bool)GetValue(HasErrorProperty);
        set => SetValue(HasErrorProperty, value);
    }
    public static readonly DependencyProperty HasErrorProperty
        = DependencyProperty.Register(
            nameof(HasError),
            typeof(bool),
            typeof(StswInputBoxTest)
        );

    /// <inheritdoc/>
    public object? Icon
    {
        get => (object?)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }
    public static readonly DependencyProperty IconProperty
        = DependencyProperty.Register(
            nameof(Icon),
            typeof(object),
            typeof(StswInputBoxTest)
        );

    ///// <inheritdoc/>
    //public string? Placeholder
    //{
    //    get => (string?)GetValue(PlaceholderProperty);
    //    set => SetValue(PlaceholderProperty, value);
    //}
    //public static readonly DependencyProperty PlaceholderProperty
    //    = DependencyProperty.Register(
    //        nameof(Placeholder),
    //        typeof(string),
    //        typeof(StswInputBoxTest)
    //    );

    /// <inheritdoc/>
    public ObservableCollection<IStswSubControl> SubControls
    {
        get => (ObservableCollection<IStswSubControl>)GetValue(SubControlsProperty);
        set => SetValue(SubControlsProperty, value);
    }
    public static readonly DependencyProperty SubControlsProperty
        = DependencyProperty.Register(
            nameof(SubControls),
            typeof(ObservableCollection<IStswSubControl>),
            typeof(StswInputBoxTest)
        );
    #endregion

    #region Overrides
    /// <inheritdoc/>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        //if (!AcceptsReturn && e.Key == Key.Enter)
        //    GetBindingExpression(TextProperty)?.UpdateSource();
    }
    #endregion
}
