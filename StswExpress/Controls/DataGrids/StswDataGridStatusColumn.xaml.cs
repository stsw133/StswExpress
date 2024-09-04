using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;

namespace StswExpress;
/// <summary>
/// 
/// </summary>
public class StswDataGridStatusColumn : DataGridTemplateColumn
{
    public new StswDataGrid? DataGridOwner { get; internal set; }

    public StswDataGridStatusColumn()
    {
        Dispatcher.CurrentDispatcher.InvokeAsync(() =>
        {
            HeaderTemplate ??= Application.Current.TryFindResource("StswDataGridStatusColumnHeaderTemplate") as DataTemplate;
            CellTemplate ??= Application.Current.TryFindResource("StswDataGridStatusColumnCellTemplate") as DataTemplate;
            //CellEditingTemplate ??= Application.Current.TryFindResource("StswDataGridStatusColumnCellEditingTemplate") as DataTemplate;

            if (DataGridOwner == null)
                return;

            var newCellStyle = new Style(typeof(DataGridCell));
            newCellStyle.Setters.Add(new Setter(Control.IsTabStopProperty, false));
            CellStyle = newCellStyle;

            CanUserReorder = false;
            CanUserResize = false;
            IsReadOnly = true;

            /// set visibility for header
            //if (specialColumn?.HeaderTemplate?.Template is TemplateContent grid)
            //    grid.Visibility = stsw.SpecialColumnVisibility == StswSpecialColumnVisibility.All ? Visibility.Visible : Visibility.Collapsed;

            /// triggers
            var style = DataGridOwner.RowStyle ?? new Style(typeof(DataGridRow));
            var newStyle = new Style(typeof(DataGridRow))
            {
                Resources = style.Resources
            };

            foreach (var setter in style.Setters)
                newStyle.Setters.Add(setter);

            foreach (var trigger in style.Triggers)
                newStyle.Triggers.Add(trigger);

            if (style.Triggers.OfType<DataTrigger>().FirstOrDefault(
                    trigger => trigger.Binding is Binding binding &&
                    binding.Path != null && binding.Path.Path == nameof(IStswCollectionItem.ShowDetails)
                ) == null)
            {
                var t = new DataTrigger() { Binding = new Binding(nameof(IStswCollectionItem.ShowDetails)), Value = true };
                t.Setters.Add(new Setter(DataGridRow.DetailsVisibilityProperty, Visibility.Visible));
                newStyle.Triggers.Add(t);
            }

            if (style.Setters.OfType<Setter>().FirstOrDefault(setter => setter.Property == DataGridRow.DetailsVisibilityProperty) == null)
                newStyle.Setters.Add(new Setter(DataGridRow.DetailsVisibilityProperty, Visibility.Collapsed));

            DataGridOwner.RowStyle = newStyle;
        }, DispatcherPriority.Loaded);
    }
}
