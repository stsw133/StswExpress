using StswExpress;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;

namespace TestApp.Modules.Main;

internal static class Q_Main
{
    /// ListOfTypes
    internal static List<string?> ListOfTypes()
    {
        var result = new DataTable();

        try
        {
            using (var sqlConn = new SqlConnection(Fn.AppDB?.GetConnString()))
            {
                sqlConn.Open();

                var query = $@"select distinct [Type]
                    from dbo.Users with(nolock)
                    order by 1";
                using (var sqlDA = new SqlDataAdapter(query, sqlConn))
                {
                    sqlDA.Fill(result);
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error ({nameof(ListOfTypes)}):{Environment.NewLine}{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        return result.AsEnumerable().Select(x => x.Field<string?>(0)).ToList();
    }

    /// GetListOfUsers
    internal static ExtCollection<M_User> GetListOfUsers(string filter, List<(string name, object val)> parameters)
    {
        var result = new DataTable();

        try
        {
            using (var sqlConn = new SqlConnection(Fn.AppDB?.GetConnString()))
            {
                sqlConn.Open();

                var query = $@"select a.ID, a.Type, a.Icon, a.Name, a.IsEnabled, a.CreateDT
                    from dbo.Users with(nolock)
                    where {filter}
                    order by a.Name";
                using (var sqlDA = new SqlDataAdapter(query, sqlConn))
                {
                    if (parameters != null)
                        foreach (var (name, val) in parameters)
                            sqlDA.SelectCommand.Parameters.AddWithValue(name, val);
                    sqlDA.Fill(result);
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error ({nameof(GetListOfUsers)}):{Environment.NewLine}{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        return result.AsList<M_User>().ToExtCollection();
    }

    /// SetListOfUsers
    internal static bool SetListOfUsers(ExtCollection<M_User> list)
    {
        var result = false;

        try
        {
            using (var sqlConn = new SqlConnection(Fn.AppDB?.GetConnString()))
            {
                sqlConn.Open();

                foreach (var item in list)
                {
                    if (item.ItemState == DataRowState.Added)
                    {
                        var query = $@"insert into dbo.Users (Type, Icon, Name, IsEnabled, CreateDT)
                            values (@Type, @Icon, @Name, @IsEnabled, @CreateDT)";
                        using (var sqlCmd = new SqlCommand(query, sqlConn))
                        {
                            sqlCmd.Parameters.AddWithValue("@Type", item.Type);
                            sqlCmd.Parameters.AddWithValue("@Icon", item.Icon);
                            sqlCmd.Parameters.AddWithValue("@Name", item.Name);
                            sqlCmd.Parameters.AddWithValue("@IsEnabled", item.IsEnabled);
                            sqlCmd.Parameters.AddWithValue("@CreateDT", item.CreateDT);
                            sqlCmd.ExecuteNonQuery();
                        }
                    }
                    else if (item.ItemState == DataRowState.Modified)
                    {
                        var query = $@"update dbo.Users
                            set Type=@Type, Icon=@Icon, Name=@Name, IsEnabled=@IsEnabled, CreateDT=@CreateDT)
                            where ID=@ID";
                        using (var sqlCmd = new SqlCommand(query, sqlConn))
                        {
                            sqlCmd.Parameters.AddWithValue("@ID", item.ID);
                            sqlCmd.Parameters.AddWithValue("@Type", item.Type);
                            sqlCmd.Parameters.AddWithValue("@Icon", item.Icon);
                            sqlCmd.Parameters.AddWithValue("@Name", item.Name);
                            sqlCmd.Parameters.AddWithValue("@IsEnabled", item.IsEnabled);
                            sqlCmd.Parameters.AddWithValue("@CreateDT", item.CreateDT);
                            sqlCmd.ExecuteNonQuery();
                        }
                    }
                    else if (item.ItemState == DataRowState.Deleted)
                    {
                        var query = $@"delete from dbo.Users
                            where ID=@ID";
                        using (var sqlCmd = new SqlCommand(query, sqlConn))
                        {
                            sqlCmd.Parameters.AddWithValue("@ID", item.ID);
                            sqlCmd.ExecuteNonQuery();
                        }
                    }
                }
                result = true;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error ({nameof(SetListOfUsers)}):{Environment.NewLine}{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        return result;
    }
}
