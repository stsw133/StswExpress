using System;

namespace TestApp.Modules.Contractors;

public class ContractorModel : StswObservableObject
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
    private byte[]? icon;
    public byte[]? Icon
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
    public bool ShowDetails { get; set; }
}
