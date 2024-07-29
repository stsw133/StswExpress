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
                    DefaultDiscount decimal(5,2),
                    IsArchival bit,
                    CreateDT datetime,
                    Pdf varbinary(max)
		        )").ExecuteNonQuery();

    /// GetContractors
    internal static IEnumerable<ContractorModel> GetContractors(string filter, IList<SqlParameter> parameters) => new StswQuery($@"
            select
                a.ID [{nameof(ContractorModel.ID)}],
                a.Type [{nameof(ContractorModel.Type)}],
                a.Icon [{nameof(ContractorModel.Icon)}],
                a.Name [{nameof(ContractorModel.Name)}],
                a.Country [{nameof(ContractorModel.Address)}/{nameof(AddressModel.Country)}],
                a.PostCode [{nameof(ContractorModel.Address)}/{nameof(AddressModel.PostCode)}],
                a.City [{nameof(ContractorModel.Address)}/{nameof(AddressModel.City)}],
                a.Street [{nameof(ContractorModel.Address)}/{nameof(AddressModel.Street)}],
                a.DefaultDiscount [{nameof(ContractorModel.DefaultDiscount)}],
                a.IsArchival [{nameof(ContractorModel.IsArchival)}],
                a.CreateDT [{nameof(ContractorModel.CreateDT)}]
            from dbo.StswExpressTEST_Contractors a with(nolock)
            where {filter ?? "1=1"}
            order by a.Name").Get<ContractorModel>(parameters);

    /// SetContractors
    internal static void SetContractors(StswBindingList<ContractorModel> list) => new StswQuery(@"
            dbo.StswExpressTEST_Contractors").Set(list, nameof(ContractorModel.ID), StswInclusionMode.Exclude,
            [nameof(ContractorModel.IconSource),
            nameof(ContractorModel.Address)]);

    /// DeleteContractor
    internal static void DeleteContractor(int id) => new StswQuery(@"
            delete from dbo.StswExpressTEST_Contractors where ID=@ID").ExecuteNonQuery([new("@ID", id)]);

    /// AddPdf
    internal static bool AddPdf(int id, byte[] file) => new StswQuery(@"
            update dbo.StswExpressTEST_Contractors
            set Pdf=@Pdf
            where ID=@ID").ExecuteNonQuery([new("@ID", id), new("@Pdf", file)]) > 0;

    /// GetPdf
    internal static byte[]? GetPdf(int id) => new StswQuery(@"
            select Pdf
            from dbo.StswExpressTEST_Contractors with(nolock)
            where ID=@ID").TryExecuteScalar<byte[]>([new("@ID", id)]);
}