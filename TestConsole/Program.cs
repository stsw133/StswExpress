global using StswExpress.Commons;
using TestConsole;

var db = new StswDatabaseModel("", "", "", "");
var stopwatch = new System.Diagnostics.Stopwatch();

// TEST 1: Fetching contractors
stopwatch.Start();
var contractors = db.Get<ContractorModel>($@"
    select Knt_GIDNumer [{nameof(ContractorModel.Id)}]
         , Knt_Akronim [{nameof(ContractorModel.Code)}]
         , Knt_Nazwa [{nameof(ContractorModel.Name)}]
         , Knt_Email [{nameof(ContractorModel.Email)}]
         , Knt_Telefon [{nameof(ContractorModel.PhoneNumber)}]
         , Knt_Archiwalny [{nameof(ContractorModel.IsArchival)}]
    from cdn.KntKarty with(nolock)");

Console.WriteLine($"Fetched {contractors.Count()} contractors in {stopwatch.ElapsedMilliseconds} ms");
stopwatch.Reset();

// TEST 2: Fetching contractors with address
stopwatch.Start();
var contractorsWithAddress = db.Get<ContractorModel>($@"
    select Knt_GIDNumer [{nameof(ContractorModel.Id)}]
         , Knt_Akronim [{nameof(ContractorModel.Code)}]
         , Knt_Nazwa [{nameof(ContractorModel.Name)}]
         , Knt_Email [{nameof(ContractorModel.Email)}]
         , Knt_Telefon [{nameof(ContractorModel.PhoneNumber)}]
         , Knt_Archiwalny [{nameof(ContractorModel.IsArchival)}]
         , Knt_Kraj [{nameof(ContractorModel.Address)}/{nameof(AddressModel.Country)}]
         , Knt_KodP [{nameof(ContractorModel.Address)}/{nameof(AddressModel.ZipCode)}]
         , Knt_Miasto [{nameof(ContractorModel.Address)}/{nameof(AddressModel.City)}]
         , Knt_Ulica [{nameof(ContractorModel.Address)}/{nameof(AddressModel.Street)}]
    from cdn.KntKarty with(nolock)");

Console.WriteLine($"Fetched {contractorsWithAddress.Count()} contractors with addresses in {stopwatch.ElapsedMilliseconds} ms");
stopwatch.Reset();
