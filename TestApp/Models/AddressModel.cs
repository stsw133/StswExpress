namespace TestApp;
public class AddressModel : StswObservableObject
{
    /// Country
    public string? Country
    {
        get => _country;
        set => SetProperty(ref _country, value);
    }
    private string? _country;

    /// PostCode
    public string? PostCode
    {
        get => _postCode;
        set => SetProperty(ref _postCode, value);
    }
    private string? _postCode;

    /// City
    public string? City
    {
        get => _city;
        set => SetProperty(ref _city, value);
    }
    private string? _city;

    /// Street
    public string? Street
    {
        get => _street;
        set => SetProperty(ref _street, value);
    }
    private string? _street;
}
