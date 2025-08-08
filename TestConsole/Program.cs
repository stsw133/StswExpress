global using StswExpress.Commons;
using Microsoft.Data.SqlClient;
using System.Data;
using TestConsole;

var db = new StswDatabaseModel("", "") { UseIntegratedSecurity = true };
var stopwatch = new System.Diagnostics.Stopwatch();

// TEST 1A: Fetching contractors into DataTable
stopwatch.Restart();
var dt = new DataTable();
using (var sqlConn = db.OpenedConnection())
{
    using var sqlDA = new SqlDataAdapter($@"
        select Knt_GIDNumer [{nameof(ContractorModel.Id)}]
             , Knt_Akronim [{nameof(ContractorModel.Code)}]
             , Knt_Nazwa1 [{nameof(ContractorModel.Name)}]
             , Knt_Email [{nameof(ContractorModel.Email)}]
             , Knt_Telefon1 [{nameof(ContractorModel.PhoneNumber)}]
             , Knt_Archiwalny [{nameof(ContractorModel.IsArchival)}]
        from cdn.KntKarty with(nolock)", sqlConn);
    sqlDA.Fill(dt);
}

stopwatch.Stop();
Console.WriteLine($"Filled DataTable with {dt.Rows.Count} contractors in {stopwatch.ElapsedMilliseconds} ms");

// TEST 1B: Mapping contractors
stopwatch.Restart();
var contractors = dt.MapTo<ContractorModel>();

stopwatch.Stop();
Console.WriteLine($"Mapped {contractors.Count()} contractors in {stopwatch.ElapsedMilliseconds} ms");

// TEST 2: Fetching contractors into IEnumerable
stopwatch.Restart();
contractors = db.Get<ContractorModel>($@"
    select Knt_GIDNumer [{nameof(ContractorModel.Id)}]
         , Knt_Akronim [{nameof(ContractorModel.Code)}]
         , Knt_Nazwa1 [{nameof(ContractorModel.Name)}]
         , Knt_Email [{nameof(ContractorModel.Email)}]
         , Knt_Telefon1 [{nameof(ContractorModel.PhoneNumber)}]
         , Knt_Archiwalny [{nameof(ContractorModel.IsArchival)}]
    from cdn.KntKarty with(nolock)");

stopwatch.Stop();
Console.WriteLine($"Fetched into IEnumerable {contractors.Count()} contractors in {stopwatch.ElapsedMilliseconds} ms");

// TEST 3A: Fetching contractors with address into DataTable
stopwatch.Restart();
dt = new DataTable();
using (var sqlConn = db.OpenedConnection())
{
    using var sqlDA = new SqlDataAdapter($@"
        select Knt_GIDNumer [{nameof(ContractorModel.Id)}]
             , Knt_Akronim [{nameof(ContractorModel.Code)}]
             , Knt_Nazwa1 [{nameof(ContractorModel.Name)}]
             , Knt_Email [{nameof(ContractorModel.Email)}]
             , Knt_Telefon1 [{nameof(ContractorModel.PhoneNumber)}]
             , Knt_Archiwalny [{nameof(ContractorModel.IsArchival)}]
             , Knt_Kraj [{nameof(ContractorModel.Address)}/{nameof(AddressModel.Country)}]
             , Knt_KodP [{nameof(ContractorModel.Address)}/{nameof(AddressModel.ZipCode)}]
             , Knt_Miasto [{nameof(ContractorModel.Address)}/{nameof(AddressModel.City)}]
             , Knt_Ulica [{nameof(ContractorModel.Address)}/{nameof(AddressModel.Street)}]
        from cdn.KntKarty with(nolock)", sqlConn);
    sqlDA.Fill(dt);
}

stopwatch.Stop();
Console.WriteLine($"Filled DataTable with {dt.Rows.Count} contractors with addresses in {stopwatch.ElapsedMilliseconds} ms");

// TEST 3B: Mapping contractors
stopwatch.Restart();
var contractorsWithAddress = dt.MapTo<ContractorModel>('/');

stopwatch.Stop();
Console.WriteLine($"Mapped {contractorsWithAddress.Count()} contractors with addresses in {stopwatch.ElapsedMilliseconds} ms");

// TEST 4: Fetching contractors with address into IEnumerable
stopwatch.Restart();
contractorsWithAddress = db.Get<ContractorModel>($@"
    select Knt_GIDNumer [{nameof(ContractorModel.Id)}]
         , Knt_Akronim [{nameof(ContractorModel.Code)}]
         , Knt_Nazwa1 [{nameof(ContractorModel.Name)}]
         , Knt_Email [{nameof(ContractorModel.Email)}]
         , Knt_Telefon1 [{nameof(ContractorModel.PhoneNumber)}]
         , Knt_Archiwalny [{nameof(ContractorModel.IsArchival)}]
         , Knt_Kraj [{nameof(ContractorModel.Address)}/{nameof(AddressModel.Country)}]
         , Knt_KodP [{nameof(ContractorModel.Address)}/{nameof(AddressModel.ZipCode)}]
         , Knt_Miasto [{nameof(ContractorModel.Address)}/{nameof(AddressModel.City)}]
         , Knt_Ulica [{nameof(ContractorModel.Address)}/{nameof(AddressModel.Street)}]
    from cdn.KntKarty with(nolock)");

stopwatch.Stop();
Console.WriteLine($"Fetched into IEnumerable {contractorsWithAddress.Count()} contractors with addresses in {stopwatch.ElapsedMilliseconds} ms");
/*
// TEST 5A: Fetching documents into IEnumerable
stopwatch.Restart();
var documents1 = db.Get<DocumentModel>($@"
    select TrN_GIDNumer [{nameof(DocumentModel.Id)}]
         , WDR_NumerDokumentu [{nameof(DocumentModel.Number)}]
         , dateadd(d, TrN_Data2, '18001228') [{nameof(DocumentModel.IssueDT)}]
         , Knt_GIDNumer [{nameof(DocumentModel.ContractorId)}]
         , Knt_Akronim [{nameof(DocumentModel.ContractorCode)}]
         , Knt_Nazwa1 [{nameof(DocumentModel.ContractorName)}]
    from cdn.TraNag with(nolock)
    join cdn.KntKarty with(nolock) on Knt_GIDTyp=TrN_KntTyp and Knt_GIDNumer=TrN_KntNumer
    where TrN_GIDTyp=2033 and TrN_Data2 >= datediff(d, '18001228', '20250401')
    order by TrN_GIDNumer desc").ToList();

stopwatch.Stop();
Console.WriteLine($"Fetched into IEnumerable {documents1.Count} documents in {stopwatch.ElapsedMilliseconds} ms");

// TEST 5B: Fetching positions into IEnumerable
stopwatch.Restart();
foreach (var document in documents1)
    document!.Positions = db.Get<DocumentPositionModel>($@"
        select TrE_GIDLp [{nameof(DocumentPositionModel.Id)}]
             , Twr_GIDNumer [{nameof(DocumentPositionModel.ArticleId)}]
             , Twr_Kod [{nameof(DocumentPositionModel.ArticleCode)}]
             , Twr_Nazwa [{nameof(DocumentPositionModel.ArticleName)}]
             , TrE_Ilosc [{nameof(DocumentPositionModel.Quantity)}]
             , TrE_CenaPoRabacie [{nameof(DocumentPositionModel.Price)}]
             , TrE_JmZ [{nameof(DocumentPositionModel.Unit)}]
        from cdn.TraElem with(nolock)
        join cdn.TwrKarty with(nolock) on Twr_GIDNumer=TrE_TwrNumer
        where TrE_GIDNumer=@DocId", new { DocId = document.Id });

stopwatch.Stop();
Console.WriteLine($"Fetched into IEnumerable {documents1.SelectMany(x => x!.Positions).Count()} positions in {stopwatch.ElapsedMilliseconds} ms");

// TEST 6: Fetching positions into DataTable
stopwatch.Restart();
dt = new DataTable();
using (var sqlConn = db.OpenedConnection())
{
    foreach (var document in documents1)
    {
        using var sqlDA = new SqlDataAdapter($@"
            select TrE_GIDLp [{nameof(DocumentPositionModel.Id)}]
                 , Twr_GIDNumer [{nameof(DocumentPositionModel.ArticleId)}]
                 , Twr_Kod [{nameof(DocumentPositionModel.ArticleCode)}]
                 , Twr_Nazwa [{nameof(DocumentPositionModel.ArticleName)}]
                 , TrE_Ilosc [{nameof(DocumentPositionModel.Quantity)}]
                 , TrE_CenaPoRabacie [{nameof(DocumentPositionModel.Price)}]
                 , TrE_JmZ [{nameof(DocumentPositionModel.Unit)}]
            from cdn.TraElem with(nolock)
            join cdn.TwrKarty with(nolock) on Twr_GIDNumer=TrE_TwrNumer
            where TrE_GIDNumer=@DocId", sqlConn);
        sqlDA.SelectCommand.Parameters.AddWithValue("@DocId", document.Id);
        dt.Clear();
        sqlDA.Fill(dt);
    }
}
stopwatch.Stop();
Console.WriteLine($"Filled DataTable with {dt.Rows.Count} positions in {stopwatch.ElapsedMilliseconds} ms");
*/

// TEST 7: Fetching documents with positions into IEnumerable (with dividing)
stopwatch.Restart();
var documents2 = db.GetDivided<DocumentModel, DocumentPositionModel>($@"
    select TrN_GIDNumer [{nameof(DocumentModel.Id)}]
         , WDR_NumerDokumentu [{nameof(DocumentModel.Number)}]
         , dateadd(d, TrN_Data2, '18001228') [{nameof(DocumentModel.IssueDT)}]
         , Knt_GIDNumer [{nameof(DocumentModel.ContractorId)}]
         , Knt_Akronim [{nameof(DocumentModel.ContractorCode)}]
         , Knt_Nazwa1 [{nameof(DocumentModel.ContractorName)}]
         , TrE_GIDNumer [{nameof(DocumentPositionModel.HeadId)}]
         , TrE_GIDLp [{nameof(DocumentPositionModel.Id)}]
         , Twr_GIDNumer [{nameof(DocumentPositionModel.ArticleId)}]
         , Twr_Kod [{nameof(DocumentPositionModel.ArticleCode)}]
         , Twr_Nazwa [{nameof(DocumentPositionModel.ArticleName)}]
         , TrE_Ilosc [{nameof(DocumentPositionModel.Quantity)}]
         , TrE_CenaPoRabacie [{nameof(DocumentPositionModel.Price)}]
         , TrE_JmZ [{nameof(DocumentPositionModel.Unit)}]
    from cdn.TraNag with(nolock)
    join cdn.KntKarty with(nolock) on Knt_GIDTyp=TrN_KntTyp and Knt_GIDNumer=TrN_KntNumer
    left join cdn.TraElem with(nolock) on TrE_GIDNumer=TrN_GIDNumer
    left join cdn.TwrKarty with(nolock) on Twr_GIDNumer=TrE_TwrNumer
    where TrN_GIDTyp=2033 and TrN_Data2 >= datediff(d, '18001228', '20250401')
    order by TrN_GIDNumer desc",
    new KeyValuePair<string, string?>(nameof(DocumentModel.Id), nameof(DocumentPositionModel.HeadId)),
    nameof(DocumentModel.Positions),
    nameof(DocumentPositionModel.HeadId));

stopwatch.Stop();
Console.WriteLine($"Fetched into IEnumerable (with dividing) {documents2.Count()} documents with positions in {stopwatch.ElapsedMilliseconds} ms");

// TEST 8: Fetching documents with positions into IEnumerable (with dividing v2)
stopwatch.Restart();
using (var sqlConn = db.OpenedConnection())
{
    var documents3 = sqlConn.Get<DocumentModel>($@"
        select TrN_GIDNumer [{nameof(DocumentModel.Id)}]
             , WDR_NumerDokumentu [{nameof(DocumentModel.Number)}]
             , dateadd(d, TrN_Data2, '18001228') [{nameof(DocumentModel.IssueDT)}]
             , Knt_GIDNumer [{nameof(DocumentModel.ContractorId)}]
             , Knt_Akronim [{nameof(DocumentModel.ContractorCode)}]
             , Knt_Nazwa1 [{nameof(DocumentModel.ContractorName)}]
        from cdn.TraNag with(nolock)
        join cdn.KntKarty with(nolock) on Knt_GIDTyp=TrN_KntTyp and Knt_GIDNumer=TrN_KntNumer
        where TrN_GIDTyp=2033 and TrN_Data2 >= datediff(d, '18001228', '20250401')
        order by TrN_GIDNumer desc", disposeConnection: false).ToList();

    sqlConn.TempTableInsert(documents3.Select(x => new { x.Id }), "#TEST");

    var positions3 = sqlConn.Get<DocumentPositionModel>($@"
        select t.Id [{nameof(DocumentPositionModel.HeadId)}]
             , TrE_GIDLp [{nameof(DocumentPositionModel.Id)}]
             , Twr_GIDNumer [{nameof(DocumentPositionModel.ArticleId)}]
             , Twr_Kod [{nameof(DocumentPositionModel.ArticleCode)}]
             , Twr_Nazwa [{nameof(DocumentPositionModel.ArticleName)}]
             , TrE_Ilosc [{nameof(DocumentPositionModel.Quantity)}]
             , TrE_CenaPoRabacie [{nameof(DocumentPositionModel.Price)}]
             , TrE_JmZ [{nameof(DocumentPositionModel.Unit)}]
        from #TEST t
        join cdn.TraElem with(nolock) on TrE_GIDNumer=t.Id
        join cdn.TwrKarty with(nolock) on Twr_GIDNumer=TrE_TwrNumer", disposeConnection: false);

    documents3.ForEach(x => x.Positions = positions3.Where(y => y.HeadId == x.Id));

    var payments3 = sqlConn.Get<DocumentPaymentModel>($@"
        select t.Id [{nameof(DocumentPaymentModel.HeadId)}]
             , TrP_Kwota [{nameof(DocumentPaymentModel.Amount)}]
        from #TEST t
        join cdn.TraPlat with(nolock) on TrP_GIDNumer=t.Id");

    documents3.ForEach(x => x.Payments = payments3.Where(y => y.HeadId == x.Id));

    stopwatch.Stop();
    Console.WriteLine($"Fetched into IEnumerable (with temp table) {documents3.Count()} documents with {positions3.Count()} positions and {payments3.Count()} payments in {stopwatch.ElapsedMilliseconds} ms");
}

// TEST 9: Fetching IDs into IEnumerable<int>
stopwatch.Restart();
var ids = db.Get<int?>($@"
    select iif(Knt_GIDNumer % 5 = 0, null, Knt_GIDNumer)
    from cdn.KntKarty with(nolock)");

stopwatch.Stop();
Console.WriteLine($"Fetched {ids.Count()} contractor IDs in {stopwatch.ElapsedMilliseconds} ms");



Console.Write("Press Enter to exit...");
Console.ReadLine();
