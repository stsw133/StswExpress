using System.ComponentModel;
using System.Windows;
using System.Windows.Data;

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
                <se:StswFilter Header="Type" FilterType="List" FilterMode="In" FilterSqlColumn="a.Type"
                               ItemsSource="{Binding Proxy.ComboSourceContractorTypes, Source={StaticResource proxy}}" DisplayMemberPath="Display" SelectedValuePath="Display"/>
            </DataGridComboBoxColumn.Header>
        </DataGridComboBoxColumn>
    </DataGrid.Columns>
</DataGrid>

*/

/// <summary>
/// Allows creating a trigger object for data binding purposes.
/// </summary>
public class StswBindingTrigger : INotifyPropertyChanged
{
    public StswBindingTrigger()
    {
        Binding = new Binding()
        {
            Source = this,
            Path = new PropertyPath(nameof(Value))
        };
    }

    /// <summary>
    /// 
    /// </summary>
    public Binding Binding { get; }

    /// <summary>
    /// 
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// 
    /// </summary>
    public void Refresh() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));

    /// <summary>
    /// 
    /// </summary>
    public object? Value { get; }
}
