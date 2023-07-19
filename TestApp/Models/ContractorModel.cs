using System;
using System.Data;
using System.IO;
using System.Windows.Media;

namespace TestApp;

public class ContractorModel : StswObservableObject, IStswCollectionItem
{
    /// ID
    private int id;
    [StswExport(nameof(ID))]
    public int ID
    {
        get => id;
        set => SetProperty(ref id, value);
    }

    /// Type
    private string? type;
    [StswExport(nameof(Type))]
    public string? Type
    {
        get => type;
        set => SetProperty(ref type, value);
    }

    /// Icon
    private ImageSource? icon;
    [StswExport(IsColumnIgnored = true)]
    public ImageSource? Icon
    {
        get => icon;
        set => SetProperty(ref icon, value);
    }

    /// Name
    private string? name;
    [StswExport(nameof(Name))]
    public string? Name
    {
        get => name;
        set => SetProperty(ref name, value);
    }

    /// Country
    private string? country;
    [StswExport(nameof(Country))]
    public string? Country
    {
        get => country;
        set => SetProperty(ref country, value);
    }

    /// PostCode
    private string? postCode;
    [StswExport("Post code")]
    public string? PostCode
    {
        get => postCode;
        set => SetProperty(ref postCode, value);
    }

    /// City
    private string? city;
    [StswExport(nameof(City))]
    public string? City
    {
        get => city;
        set => SetProperty(ref city, value);
    }

    /// Street
    private string? street;
    [StswExport(nameof(Street))]
    public string? Street
    {
        get => street;
        set => SetProperty(ref street, value);
    }

    /// IsArchival
    private bool isArchival;
    [StswExport("Is archival", "{0:yes;1;no}")]
    public bool IsArchival
    {
        get => isArchival;
        set => SetProperty(ref isArchival, value);
    }

    /// CreateDT
    private DateTime createDT = DateTime.Now;
    [StswExport("Date of creation", "yyyy-MM-dd")]
    public DateTime CreateDT
    {
        get => createDT;
        set => SetProperty(ref createDT, value);
    }

    /// Pdf
    private string? pdf;
    [StswExport(IsColumnIgnored = true)]
    public string? Pdf
    {
        get => pdf;
        set => SetProperty(ref pdf, value);
    }

    /// > IStswCollectionItem ...
    /// ErrorMessage
    private string? errorMessage;
    [StswExport(IsColumnIgnored = true)]
    public string? ErrorMessage
    {
        get => errorMessage;
        set => SetProperty(ref errorMessage, value);
    }

    /// ItemState
    private DataRowState itemState;
    [StswExport(IsColumnIgnored = true)]
    public DataRowState ItemState
    {
        get => itemState;
        set => SetProperty(ref itemState, value);
    }

    /// ShowDetails
    private bool? showDetails = false;
    [StswExport(IsColumnIgnored = true)]
    public bool? ShowDetails
    {
        get => showDetails;
        set
        {
            SetProperty(ref showDetails, value);
            if (value == true)
            {
                var path = Path.Combine(Path.GetTempPath(), $"Contractor_{ID}.pdf");
                var file = SQL.GetPdf(ID);
                if (file != null)
                {
                    File.WriteAllBytes(path, file);
                    Pdf = path;
                }
            }
        }
    }
}
