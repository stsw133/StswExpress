using System.Windows;

namespace StswExpress;

/// <summary>
/// Allows creating a proxy object for data binding purposes.
/// </summary>
public class StswBindingProxy : Freezable
{
    protected override Freezable CreateInstanceCore() => new StswBindingProxy();

    /// Proxy
    public static readonly DependencyProperty ProxyProperty
        = DependencyProperty.Register(
            nameof(Proxy),
            typeof(object),
            typeof(StswBindingProxy)
        );
    public object Proxy
    {
        get => GetValue(ProxyProperty);
        set => SetValue(ProxyProperty, value);
    }
}

/* usage:

<DataGrid Margin="3" ItemsSource="{Binding ListContractors}">
    <DataGrid.Resources>
        <se:StswBindingProxy x:Key="proxy" Proxy="{Binding}"/>
    </DataGrid.Resources>
    <DataGrid.Columns>
        <DataGridTextColumn Binding="{Binding Name}">
            <DataGridTextColumn.Header>
                <se:StswFilter Header="Name" FilterType="Text" FilterMode="Contains" FilterSqlColumn="c.Name"/>
            </DataGridTextColumn.Header>
        </DataGridTextColumn>
    </DataGrid.Columns>
</DataGrid>

*/