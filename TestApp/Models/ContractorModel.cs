using System;
using System.Data;
using System.Windows.Media;

namespace TestApp;

public class ContractorModel : StswObservableObject, IStswCollectionItem
{
    /// ID
    [StswExport(nameof(ID))]
    public int ID
    {
        get => id;
        set => SetProperty(ref id, value);
    }
    private int id;

    /// Type
    [StswExport(nameof(Type))]
    public string? Type
    {
        get => type;
        set => SetProperty(ref type, value);
    }
    private string? type;

    /// Icon
    public byte[]? Icon
    {
        get => icon;
        set
        {
            SetProperty(ref icon, value);
            if (value != null)
                IconSource = value.ToBitmapImage();
        }
    }
    private byte[]? icon;

    /// IconSource
    public ImageSource? IconSource
    {
        get => iconSource;
        set => SetProperty(ref iconSource, value);
    }
    private ImageSource? iconSource;

    /// Name
    [StswExport(nameof(Name))]
    public string? Name
    {
        get => name;
        set => SetProperty(ref name, value);
    }
    private string? name;

    /// Country
    [StswExport(nameof(Country))]
    public string? Country
    {
        get => country;
        set => SetProperty(ref country, value);
    }
    private string? country;

    /// PostCode
    [StswExport("Post code")]
    public string? PostCode
    {
        get => postCode;
        set => SetProperty(ref postCode, value);
    }
    private string? postCode;

    /// City
    [StswExport(nameof(City))]
    public string? City
    {
        get => city;
        set => SetProperty(ref city, value);
    }
    private string? city;

    /// Street
    [StswExport(nameof(Street))]
    public string? Street
    {
        get => street;
        set => SetProperty(ref street, value);
    }
    private string? street;

    /// IsArchival
    [StswExport("Is archival", "{0:yes;1;no}")]
    public bool IsArchival
    {
        get => isArchival;
        set => SetProperty(ref isArchival, value);
    }
    private bool isArchival;

    /// CreateDT
    [StswExport("Date of creation", "yyyy-MM-dd")]
    public DateTime CreateDT
    {
        get => createDT;
        set => SetProperty(ref createDT, value);
    }
    private DateTime createDT = DateTime.Now;

    /// > IStswCollectionItem ...
    /// ItemMessage
    public string? ItemMessage
    {
        get => itemMessage;
        set => SetProperty(ref itemMessage, value);
    }
    private string? itemMessage;

    /// ItemState
    public DataRowState ItemState
    {
        get => itemState;
        set => SetProperty(ref itemState, value);
    }
    private DataRowState itemState;

    /// ShowDetails
    public bool? ShowDetails
    {
        get => showDetails;
        set => SetProperty(ref showDetails, value);
    }
    private bool? showDetails = false;
}
