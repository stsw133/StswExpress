using System;
using System.Windows.Media;

namespace TestApp;

public class ArticleModel : StswObservableObject, IStswCollectionItem
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
    public ArticleType Type
    {
        get => _type;
        set => SetProperty(ref _type, value);
    }
    private ArticleType _type;

    /// Icon
    public byte[]? Icon
    {
        get => _icon;
        set => SetProperty(ref _icon, value, () => IconSource ??= StswFnUI.BytesToBitmapImage(value));
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

    /// EAN
    [StswExport(nameof(EAN))]
    public string? EAN
    {
        get => _ean;
        set => SetProperty(ref _ean, value);
    }
    private string? _ean;
    
    /// UoM
    [StswExport("Unit of measure")]
    public string? UoM
    {
        get => _uom;
        set => SetProperty(ref _uom, value);
    }
    private string? _uom;

    /// Weight
    [StswExport(nameof(Weight))]
    public decimal Weight
    {
        get => _weight;
        set => SetProperty(ref _weight, value);
    }
    private decimal _weight;

    /// WeightUoM
    [StswExport(nameof(WeightUoM))]
    public string? WeightUoM
    {
        get => _weightUoM;
        set => SetProperty(ref _weightUoM, value);
    }
    private string? _weightUoM;
    
    /// GrossWeight
    [StswExport(nameof(GrossWeight))]
    public decimal GrossWeight
    {
        get => _grossWeight;
        set => SetProperty(ref _grossWeight, value);
    }
    private decimal _grossWeight;

    /// GrossWeightUoM
    [StswExport(nameof(GrossWeightUoM))]
    public string? GrossWeightUoM
    {
        get => _grossWeightUoM;
        set => SetProperty(ref _grossWeightUoM, value);
    }
    private string? _grossWeightUoM;
    
    /// DiscountType
    [StswExport(nameof(DiscountType))]
    public DiscountType DiscountType
    {
        get => _discountType;
        set => SetProperty(ref _discountType, value);
    }
    private DiscountType _discountType;
    
    /// ProducerID
    [StswExport("Producer ID")]
    public int ProducerID
    {
        get => _producerID;
        set => SetProperty(ref _producerID, value);
    }
    private int _producerID;

    /// IsArchival
    [StswExport("Is archival", "yes~no")]
    public bool IsArchival
    {
        get => _isArchival;
        set => SetProperty(ref _isArchival, value);
    }
    private bool _isArchival;

    /// CreatorID
    [StswExport("Creator ID")]
    public int CreatorID
    {
        get => _creatorID;
        set => SetProperty(ref _creatorID, value);
    }
    private int _creatorID;
    
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
