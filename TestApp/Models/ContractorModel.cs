using System;
using System.Windows.Media;

namespace TestApp;

public class ContractorModel : StswObservableObject, IStswCollectionItem
{
    /// ID
    [StswExport(nameof(ID))]
    public int ID
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }
    private int _id;

    /// Type
    [StswExport(nameof(Type))]
    public ContractorType Type
    {
        get => _type;
        set => SetProperty(ref _type, value);
    }
    private ContractorType _type;

    /// Icon
    public byte[]? Icon
    {
        get => _icon;
        set => SetProperty(ref _icon, value, () => IconSource ??= value?.ToBitmapImage());
    }
    private byte[]? _icon;

    /// IconSource
    public ImageSource? IconSource
    {
        get => _iconSource;
        set => SetProperty(ref _iconSource, value, () => Icon = value?.ToBytes());
    }
    private ImageSource? _iconSource;

    /// Name
    [StswExport(nameof(Name))]
    public string? Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }
    private string? _name;

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

    /// IsArchival
    [StswExport("Is archival")]
    public bool IsArchival
    {
        get => _isArchival;
        set => SetProperty(ref _isArchival, value);
    }
    private bool _isArchival;

    /// CreateDT
    [StswExport("Date of creation", "yyyy-MM-dd")]
    public DateTime CreateDT
    {
        get => _createDT;
        set => SetProperty(ref _createDT, value);
    }
    private DateTime _createDT = DateTime.Now;

    /// ItemState
    public StswItemState ItemState
    {
        get => _itemState;
        set => SetProperty(ref _itemState, value);
    }
    private StswItemState _itemState;

    /// ShowDetails
    public bool? ShowDetails
    {
        get => _showDetails;
        set => SetProperty(ref _showDetails, value);
    }
    private bool? _showDetails = false;
}
