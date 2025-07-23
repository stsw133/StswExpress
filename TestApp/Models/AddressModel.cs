namespace TestApp;
public partial class AddressModel : StswObservableObject
{
    [StswObservableProperty] string? _city;
    [StswObservableProperty] string? _country;
    [StswObservableProperty] string? _postCode;
    [StswObservableProperty] string? _street;
}
