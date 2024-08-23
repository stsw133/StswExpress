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
        set => SetProperty(ref _icon, value, () => IconSource ??= StswFn.ToBitmapImage(value));
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

    /// Address
    [StswExport(nameof(Address))]
    public AddressModel Address
    {
        get => _address;
        set => SetProperty(ref _address, value);
    }
    private AddressModel _address = new();

    /// DefaultDiscount
    [StswExport(nameof(DefaultDiscount))]
    public decimal DefaultDiscount
    {
        get => _defaultDiscount;
        set => SetProperty(ref _defaultDiscount, value);
    }
    private decimal _defaultDiscount;

    /// IsArchival
    [StswExport("Is archival", "yes~no")]
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
