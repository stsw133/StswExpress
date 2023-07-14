using System.Windows;
using System.Windows.Controls;

namespace StswExpress;

public interface IStswComponent
{
    public GridLength? IconScale { get; set; }
}
