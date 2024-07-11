using System.Windows;

namespace StswExpress;

/// <summary>
/// Allows creating a proxy object for data binding purposes.
/// </summary>
public class StswBindingProxy : Freezable
{
    public StswBindingProxy()
    {
    }
    public StswBindingProxy(object proxy)
    {
        Proxy = proxy;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
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

/* usage:

<DataGrid Margin="3" ItemsSource="{Binding ListContractors}">
    <DataGrid.Resources>
        <se:StswBindingProxy x:Key="proxy" Proxy="{Binding}"/>
    </DataGrid.Resources>
    <DataGrid.Columns>
        <DataGridComboBoxColumn TextBinding="{Binding Type}" ItemsSource="{Binding Proxy.ComboSourceContractorTypes, Source={StaticResource proxy}}" DisplayMemberPath="Display" SelectedValuePath="Display">
            <DataGridComboBoxColumn.Header>
                <se:StswFilterSql Header="Type" FilterType="List" FilterMode="In" FilterValuePath="a.Type"
                                  ItemsSource="{Binding Proxy.ComboSourceContractorTypes, Source={StaticResource proxy}}" DisplayMemberPath="Display" SelectedValuePath="Display"/>
            </DataGridComboBoxColumn.Header>
        </DataGridComboBoxColumn>
    </DataGrid.Columns>
</DataGrid>

*/
