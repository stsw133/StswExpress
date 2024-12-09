using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;

namespace TestApp;

internal static class SQL
{
    internal static StswDatabaseModel DbCurrent;

    static SQL()
    {
        DbCurrent = StswDatabases.ImportList().First();
    }

    /// InitializeTables
    internal static void InitializeContractorsTables() => DbCurrent.ExecuteNonQuery(@"
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
	        )");

    /// GetContractors
    internal static IEnumerable<ContractorModel> GetContractors(StswDataGridFiltersDataModel filter) => DbCurrent.Get<ContractorModel>($@"
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
        where {filter.SqlFilter ?? "1=1"}
        order by a.Name", filter.SqlParameters)!;

    /// SetContractors
    internal static void SetContractors(StswBindingList<ContractorModel> list) => DbCurrent.Set(list,
        "dbo.StswExpressTEST_Contractors", typeof(ContractorModel).GetProperties().Select(x => x.Name).Except([
            nameof(ContractorModel.ID),
            nameof(ContractorModel.IconSource),
            nameof(ContractorModel.Address)
        ]));

    /// DeleteContractor
    internal static void DeleteContractor(int id) => DbCurrent.ExecuteNonQuery(@"
        delete from dbo.StswExpressTEST_Contractors where ID=@ID", new { ID = id });

    /// AddPdf
    internal static bool AddPdf(int id, byte[] file) => DbCurrent.ExecuteNonQuery(@"
        update dbo.StswExpressTEST_Contractors
        set Pdf=@File
        where ID=@ID", new { ID = id, File = file }) > 0;

    /// GetPdf
    internal static byte[]? GetPdf(int id) => DbCurrent.ExecuteScalar<byte[]?>(@"
        select Pdf
        from dbo.StswExpressTEST_Contractors with(nolock)
        where ID=@ID", new { ID = id });

    /// GetContractorAddress
    internal static AddressModel? GetContractorAddress(int contractorID)
    {
        using (var sqlDR = DbCurrent.ExecuteReader($@"
            select
                a.Country [{nameof(AddressModel.Country)}],
                a.PostCode [{nameof(AddressModel.PostCode)}],
                a.City [{nameof(AddressModel.City)}],
                a.Street [{nameof(AddressModel.Street)}]
            from dbo.StswExpressTEST_Contractors a with(nolock)
            where a.ID=@contractorID", new { contractorID }))
        {
            if (sqlDR.Read())
            {
                return new()
                {
                    City = sqlDR[nameof(AddressModel.City)].ConvertTo<string>(),
                    Country = sqlDR[nameof(AddressModel.Country)].ConvertTo<string>(),
                    PostCode = sqlDR[nameof(AddressModel.PostCode)].ConvertTo<string>(),
                    Street = sqlDR[nameof(AddressModel.Street)].ConvertTo<string>()
                };
            }
        }

        return null;
    }
}
