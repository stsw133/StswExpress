namespace TestApp;
public class AddressModel : StswObservableObject
{
    /// Country
    [StswExport(nameof(Country))]
    public string? Country
    {
        get => _country;
        set => SetProperty(ref _country, value);
    }
    private string? _country;

    /// PostCode
    [StswExport("Post code")]
    public string? PostCode
    {
        get => _postCode;
        set => SetProperty(ref _postCode, value);
    }
    private string? _postCode;

    /// City
    [StswExport(nameof(City))]
    public string? City
    {
        get => _city;
        set => SetProperty(ref _city, value);
    }
    private string? _city;

    /// Street
    [StswExport(nameof(Street))]
    public string? Street
    {
        get => _street;
        set => SetProperty(ref _street, value);
    }
    private string? _street;
}
