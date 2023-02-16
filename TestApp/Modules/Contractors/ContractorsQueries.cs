using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.Windows;

namespace TestApp.Modules.Contractors;

internal static class ContractorsQueries
{
    /// InitializeTables
    internal static void InitializeTables()
    {
        if (DesignerProperties.GetIsInDesignMode(Application.Current.MainWindow) || string.IsNullOrEmpty(StswFn.AppDB?.Server))
            return;

        try
        {
            using (var sqlConn = new SqlConnection(StswFn.AppDB?.GetConnString()))
            {
                sqlConn.Open();

                var query = @"
                    if not exists (select * from sysobjects where name='StswExpressTEST_Contractors' and xtype='U')
				        create table StswExpressTEST_Contractors
				        (
				    	    ID int identity(1,1) not null primary key,
                            Type varchar(30),
                            Icon varbinary(max),
                            Name varchar(255),
                            Country varchar(2),
                            PostCode varchar(10),
                            City varchar(30),
                            Street varchar(60),
                            IsArchival bit,
                            CreateDT datetime
				        )";
                using (var sqlCmd = new SqlCommand(query, sqlConn))
                    sqlCmd.ExecuteNonQuery();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error ({nameof(InitializeTables)}):{Environment.NewLine}{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// ListOfTypes
    internal static List<string?> ListOfTypes()
    {
        return new List<string?>() {
            "Test1",
            "Test2",
            "Test3",
            null
        };
        /*
        var result = new DataTable();

        try
        {
            using (var sqlConn = new SqlConnection(StswFn.AppDB?.GetConnString()))
            {
                sqlConn.Open();

                var query = $@"
                    select distinct [Type]
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
            MessageBox.Show($"Error ({MethodBase.GetCurrentMethod()?.Name}):{Environment.NewLine}{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        
        return result.AsEnumerable().Select(x => x.Field<string?>(0)).ToList();
        */
    }

    /// GetContractors
    internal static StswCollection<ContractorModel> GetContractors(string filter, List<(string name, object val)> parameters)
    {
        var result = new DataTable();
        
        try
        {
            using (var sqlConn = new SqlConnection(StswFn.AppDB?.GetConnString()))
            {
                sqlConn.Open();

                var query = $@"
                    select
                        a.ID,
                        a.Type,
                        a.Icon,
                        a.Name,
                        a.Country,
                        a.PostCode,
                        a.City,
                        a.Street,
                        a.IsArchival,
                        a.CreateDT
                    from dbo.StswExpressTEST_Contractors a with(nolock)
                    where {filter}
                    order by a.Name";
                using (var sqlDA = new SqlDataAdapter(query, sqlConn))
                {
                    foreach (var (name, val) in parameters)
                        sqlDA.SelectCommand.Parameters.AddWithValue(name, val);
                    sqlDA.Fill(result);
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error ({MethodBase.GetCurrentMethod()?.Name}):{Environment.NewLine}{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        
        return result.AsList<ContractorModel>().ToStswCollection();
    }

    /// SetContractors
    internal static bool SetContractors(StswCollection<ContractorModel> list)
    {
        var result = false;
        
        try
        {
            using (var sqlConn = new SqlConnection(StswFn.AppDB?.GetConnString()))
            {
                sqlConn.Open();

                foreach (var item in list.GetItemsByState(DataRowState.Added))
                {
                    var query = $@"
                        insert into dbo.StswExpressTEST_Contractors
                            (Type, Icon, Name, Country, PostCode, City, Street,
                             IsArchival, CreateDT)
                        values
                            (@Type, @Icon, @Name, @Country, @PostCode, @City, @Street,
                             @IsArchival, @CreateDT)";
                    using (var sqlCmd = new SqlCommand(query, sqlConn))
                    {
                        sqlCmd.Parameters.AddWithValue("@Type", (object?)item.Type ?? DBNull.Value);
                        sqlCmd.Parameters.Add("@Icon", SqlDbType.VarBinary, 8000).Value = (object?)item.Icon ?? DBNull.Value;
                        sqlCmd.Parameters.AddWithValue("@Name", (object?)item.Name ?? DBNull.Value);
                        sqlCmd.Parameters.AddWithValue("@Country", (object?)item.Country ?? DBNull.Value);
                        sqlCmd.Parameters.AddWithValue("@PostCode", (object?)item.PostCode ?? DBNull.Value);
                        sqlCmd.Parameters.AddWithValue("@City", (object?)item.City ?? DBNull.Value);
                        sqlCmd.Parameters.AddWithValue("@Street", (object?)item.Street ?? DBNull.Value);
                        sqlCmd.Parameters.AddWithValue("@IsArchival", (object?)item.IsArchival ?? DBNull.Value);
                        sqlCmd.Parameters.AddWithValue("@CreateDT", (object?)item.CreateDT ?? DBNull.Value);
                        sqlCmd.ExecuteNonQuery();
                    }
                }
                foreach (var item in list.GetItemsByState(DataRowState.Modified))
                {
                    var query = $@"
                        update dbo.StswExpressTEST_Contractors
                        set Type=@Type, Icon=@Icon, Name=@Name,
                            Country=@Country, PostCode=@PostCode, City=@City, Street=@Street,
                            IsArchival=@IsArchival, CreateDT=@CreateDT
                        where ID=@ID";
                    using (var sqlCmd = new SqlCommand(query, sqlConn))
                    {
                        sqlCmd.Parameters.AddWithValue("@ID", item.ID);
                        sqlCmd.Parameters.AddWithValue("@Type", (object?)item.Type ?? DBNull.Value);
                        sqlCmd.Parameters.Add("@Icon", SqlDbType.VarBinary, 8000).Value = (object?)item.Icon ?? DBNull.Value;
                        sqlCmd.Parameters.AddWithValue("@Name", (object?)item.Name ?? DBNull.Value);
                        sqlCmd.Parameters.AddWithValue("@Country", (object?)item.Country ?? DBNull.Value);
                        sqlCmd.Parameters.AddWithValue("@PostCode", (object?)item.PostCode ?? DBNull.Value);
                        sqlCmd.Parameters.AddWithValue("@City", (object?)item.City ?? DBNull.Value);
                        sqlCmd.Parameters.AddWithValue("@Street", (object?)item.Street ?? DBNull.Value);
                        sqlCmd.Parameters.AddWithValue("@IsArchival", (object?)item.IsArchival ?? DBNull.Value);
                        sqlCmd.Parameters.AddWithValue("@CreateDT", (object?)item.CreateDT ?? DBNull.Value);
                        sqlCmd.ExecuteNonQuery();
                    }
                }
                foreach (var item in list.GetItemsByState(DataRowState.Deleted))
                {
                    var query = $@"
                        delete from dbo.StswExpressTEST_Contractors
                        where ID=@ID";
                    using (var sqlCmd = new SqlCommand(query, sqlConn))
                    {
                        sqlCmd.Parameters.AddWithValue("@ID", item.ID);
                        sqlCmd.ExecuteNonQuery();
                    }
                }

                result = true;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error ({MethodBase.GetCurrentMethod()?.Name}):{Environment.NewLine}{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        
        return result;
    }

    /// DeleteContractor
    internal static bool DeleteContractor(int id)
    {
        var result = false;
        
        try
        {
            using (var sqlConn = new SqlConnection(StswFn.AppDB?.GetConnString()))
            {
                sqlConn.Open();

                var query = $@"
                    delete from dbo.StswExpressTEST_Contractors
                    where ID=@ID";
                using (var sqlCmd = new SqlCommand(query, sqlConn))
                {
                    sqlCmd.Parameters.AddWithValue("@ID", id);
                    sqlCmd.ExecuteNonQuery();
                }

                result = true;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error ({MethodBase.GetCurrentMethod()?.Name}):{Environment.NewLine}{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        
        return result;
    }
}
