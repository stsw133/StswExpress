using System.Windows;

namespace StswExpress;
/// <summary>
/// Allows creating a proxy object for data binding purposes.
/// </summary>
/// <example>
/// The following example demonstrates how to use the class:
/// <code>
/// &lt;DataGrid Margin="3" ItemsSource="{Binding ListContractors}"&gt;
///     &lt;DataGrid.Resources&gt;
///         &lt;se:StswBindingProxy x:Key="proxy" Proxy="{Binding}"/&gt;
///     &lt;/DataGrid.Resources&gt;
///     &lt;DataGrid.Columns&gt;
///         &lt;DataGridComboBoxColumn TextBinding="{Binding Type}" ItemsSource="{Binding Proxy.ComboSourceContractorTypes, Source={StaticResource proxy}}" DisplayMemberPath="Display" SelectedValuePath="Display"&gt;
///             &lt;DataGridComboBoxColumn.Header&gt;
///                 &lt;se:StswFilterSql Header="Type" FilterType="List" FilterMode="In" FilterValuePath="a.Type"
///                                      ItemsSource="{Binding Proxy.ComboSourceContractorTypes, Source={StaticResource proxy}}" DisplayMemberPath="Display" SelectedValuePath="Display"/&gt;
///             &lt;/DataGridComboBoxColumn.Header&gt;
///         &lt;/DataGridComboBoxColumn&gt;
///     &lt;/DataGrid.Columns&gt;
/// &lt;/DataGrid&gt;
/// </code>
/// </example>
public class StswBindingProxy : Freezable
{
    /// <inheritdoc/>
    protected override Freezable CreateInstanceCore() => new StswBindingProxy();

    /// <summary>
    /// Gets or sets the proxy object for data binding.
    /// </summary>
    public object Proxy
    {
        get => GetValue(ProxyProperty);
        set => SetValue(ProxyProperty, value);
    }
    public static readonly DependencyProperty ProxyProperty
        = DependencyProperty.Register(
            nameof(Proxy),
            typeof(object),
            typeof(StswBindingProxy)
        );
}
