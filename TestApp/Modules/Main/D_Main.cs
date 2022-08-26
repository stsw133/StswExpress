using StswExpress;
using System.Collections.Generic;
using TestApp.Models;

namespace TestApp.Modules.Main;
public class D_Main : VM
{
    /// Filter SQL
    public string FilterSqlString { get; set; } = string.Empty;
    public List<(string name, object val)> FilterSqlParams { get; set; } = new();

    /// LoadingProgress
    private double loadingProgress = 100;
    public double LoadingProgress
    {
        get => loadingProgress;
        set => SetField(ref loadingProgress, value, () => LoadingProgress);
    }

    /// ListTest
    private List<M_Test> listTest = new();
    public List<M_Test> ListTest
    {
        get => listTest;
        set => SetField(ref listTest, value, () => ListTest);
    }
}
