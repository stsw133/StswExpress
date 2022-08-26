using StswExpress;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using TestApp.Models;

namespace TestApp.Modules.Main;
internal static class Q_Main
{
    /// GetListTest
    internal static List<M_Test> GetListTest(string filter, List<(string name, object val)> parameters)
    {
        var result = new List<M_Test>();

        try
        {
            using (var sqlConn = new SqlConnection(Fn.AppDatabase?.GetMssqlConnString()))
            {
                var query = $@"select a.ID, a.Icon, a.Name, a.IsEnabled, a.CreateDT
                    from dbo.Test with(nolock)
                    where {filter}
                    order by a.Name";
                using (var sqlDA = new SqlDataAdapter(query, sqlConn))
                {
                    if (parameters != null)
                        foreach (var param in parameters)
                            sqlDA.SelectCommand.Parameters.AddWithValue(param.name, param.val);

                    var dt = new DataTable();
                    sqlDA.Fill(dt);
                    result = dt.ToList<M_Test>();
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message} ({nameof(GetListTest)})", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        return result;
    }
}
