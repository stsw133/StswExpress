using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace TestApp;

internal static class SQL
{
    static SQL()
    {
        if (StswDatabases.Current == null)
        {
            StswDatabases.ImportList();
            StswDatabases.Current = StswDatabases.Collection.FirstOrDefault();
        }
    }

    /// InitializeTables
    internal static void InitializeContractorsTables() => new StswQuery(@"
            if not exists (select 1 from sysobjects where name='StswExpressTEST_Contractors' and xtype='U')
		        create table StswExpressTEST_Contractors
		        (
		    	    ID int identity(1,1) not null primary key,
                    Type int,
                    Icon varbinary(max),
                    Name varchar(255),
                    Country varchar(2),
                    PostCode varchar(10),
                    City varchar(30),
                    Street varchar(60),
                    IsArchival bit,
                    CreateDT datetime,
                    Pdf varbinary(max)
		        )")
        .ExecuteNonQuery();

    /// GetContractors
    internal static IEnumerable<ContractorModel>? GetContractors(string filter, IList<SqlParameter> parameters) => new StswQuery($@"
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
            where {filter ?? "1=1"}
            order by a.Name")
        .Get<ContractorModel>(parameters);

    /// SetContractors
    internal static void SetContractors(StswBindingList<ContractorModel> list)
    {
        /*
            using (var sqlConn = new SqlConnection(StswDatabases.Current?.GetConnString()))
            {
                sqlConn.Open();

                foreach (var item in list.GetItemsByState(StswItemState.Added))
                {
                    var query = StswQuery.LessSpaceQuery($@"
                        insert into dbo.StswExpressTEST_Contractors
                            (Type, Icon, Name, Country, PostCode, City, Street,
                             IsArchival, CreateDT)
                        values
                            (@Type, @Icon, @Name, @Country, @PostCode, @City, @Street,
                             @IsArchival, @CreateDT)");
                    using (var sqlCmd = new SqlCommand(query, sqlConn))
                    {
                        sqlCmd.Parameters.AddWithValue("@Type", (object?)item.Type ?? DBNull.Value);
                        sqlCmd.Parameters.Add("@Icon", SqlDbType.VarBinary).Value = (object?)item.IconSource?.ToBytes() ?? DBNull.Value;
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
                foreach (var item in list.GetItemsByState(StswItemState.Modified))
                {
                    var query = StswQuery.LessSpaceQuery($@"
                        update dbo.StswExpressTEST_Contractors
                        set Type=@Type, Icon=@Icon, Name=@Name,
                            Country=@Country, PostCode=@PostCode, City=@City, Street=@Street,
                            IsArchival=@IsArchival, CreateDT=@CreateDT
                        where ID=@ID");
                    using (var sqlCmd = new SqlCommand(query, sqlConn))
                    {
                        sqlCmd.Parameters.AddWithValue("@ID", item.ID);
                        sqlCmd.Parameters.AddWithValue("@Type", (object?)item.Type ?? DBNull.Value);
                        sqlCmd.Parameters.Add("@Icon", SqlDbType.VarBinary).Value = (object?)item.IconSource?.ToBytes() ?? DBNull.Value;
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
                foreach (var item in list.GetItemsByState(StswItemState.Deleted))
                {
                    var query = StswQuery.LessSpaceQuery($@"
                        delete from dbo.StswExpressTEST_Contractors
                        where ID=@ID");
                    using (var sqlCmd = new SqlCommand(query, sqlConn))
                    {
                        sqlCmd.Parameters.AddWithValue("@ID", item.ID);
                        sqlCmd.ExecuteNonQuery();
                    }
                }

                result = true;
            }

        return true;
        */

        new StswQuery("dbo.StswExpressTEST_Contractors").Set(list, nameof(ContractorModel.ID), StswInclusionMode.Exclude, [nameof(ContractorModel.IconSource)]);
    }

    /// DeleteContractor
    internal static void DeleteContractor(int id) => new StswQuery(@"
            delete from dbo.StswExpressTEST_Contractors where ID=@ID")
        .ExecuteNonQuery([new("@ID", id)]);

    /// AddPdf
    internal static bool AddPdf(int id, byte[] file) => new StswQuery(@"
            update dbo.StswExpressTEST_Contractors
            set Pdf=@Pdf
            where ID=@ID")
        .ExecuteNonQuery([new("@ID",id), new("@Pdf",file)]) > 0;

    /// GetPdf
    internal static byte[]? GetPdf(int id) => new StswQuery(@"
            select Pdf
            from dbo.StswExpressTEST_Contractors with(nolock)
            where ID=@ID")
        .TryExecuteScalar<byte[]>([new("@ID",id)]);
}
