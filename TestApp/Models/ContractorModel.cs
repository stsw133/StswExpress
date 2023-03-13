using System;
using System.IO;
using System.Windows.Media;

namespace TestApp;

public class ContractorModel : StswCollectionItem
{
    /// ID
    private int id;
    public int ID
    {
        get => id;
        set => SetProperty(ref id, value);
    }

    /// Type
    private string? type;
    public string? Type
    {
        get => type;
        set => SetProperty(ref type, value);
    }

    /// Icon
    private ImageSource? icon;
    public ImageSource? Icon
    {
        get => icon;
        set => SetProperty(ref icon, value);
    }

    /// Name
    private string? name;
    public string? Name
    {
        get => name;
        set => SetProperty(ref name, value);
    }

    /// Country
    private string? country;
    public string? Country
    {
        get => country;
        set => SetProperty(ref country, value);
    }

    /// PostCode
    private string? postCode;
    public string? PostCode
    {
        get => postCode;
        set => SetProperty(ref postCode, value);
    }

    /// City
    private string? city;
    public string? City
    {
        get => city;
        set => SetProperty(ref city, value);
    }

    /// Street
    private string? street;
    public string? Street
    {
        get => street;
        set => SetProperty(ref street, value);
    }

    /// IsArchival
    private bool isArchival;
    public bool IsArchival
    {
        get => isArchival;
        set => SetProperty(ref isArchival, value);
    }

    /// CreateDT
    private DateTime createDT = DateTime.Now;
    public DateTime CreateDT
    {
        get => createDT;
        set => SetProperty(ref createDT, value);
    }

    /// ShowDetails
    private bool showDetails;
    public new bool ShowDetails
    {
        get => showDetails;
        set
        {
            SetProperty(ref showDetails, value);
            if (value)
            {
                var path = Path.Combine(Path.GetTempPath(), $"Contractor_{ID}.pdf");
                var file = ContractorsQueries.GetPdf(ID);
                if (file != null)
                {
                    File.WriteAllBytes(path, file);
                    Pdf = path;
                }
            }
        }
    }

    /// Pdf
    private string? pdf;
    public string? Pdf
    {
        get => pdf;
        set => SetProperty(ref pdf, value);
    }
}
