using StswExpress;
using System.Collections.Generic;

namespace TestApp.Modules.Main;

public class D_Settings : BaseD
{
    /// Filter SQL
    public string FilterSqlString { get; set; } = string.Empty;
    public List<(string name, object val)> FilterSqlParams { get; set; } = new();

    /// LoadingProgress
    private double loadingProgress = 0;
    public double LoadingProgress
    {
        get => loadingProgress;
        set => SetField(ref loadingProgress, value, () => LoadingProgress);
    }

    /// ComboLists
    public List<string?> ListTypes => Q_Main.ListOfTypes();

    /// ListUsers
    private ExtCollection<M_User> listUsers = new();
    public ExtCollection<M_User> ListUsers
    {
        get => listUsers;
        set => SetField(ref listUsers, value, () => ListUsers);
    }
}
