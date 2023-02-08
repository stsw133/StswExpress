using System;

namespace TestApp.Modules.Contractors;

public class ContractorModel
{
    /// ID
    [StswExcelAttribute("ID")]
    public int ID { get; set; } = default;

    /// Type
    [StswExcelAttribute("Type")]
    public string? Type { get; set; } = default;
    
    /// Icon
    public byte[]? Icon { get; set; } = default;

    /// Name
    [StswExcelAttribute("Name")]
    public string? Name { get; set; } = default;

    /// Country
    [StswExcelAttribute("Country")]
    public string? Country { get; set; } = default;
    
    /// PostCode
    [StswExcelAttribute("Post code")]
    public string? PostCode { get; set; } = default;

    /// City
    [StswExcelAttribute("City")]
    public string? City { get; set; } = default;

    /// Street
    [StswExcelAttribute("Street")]
    public string? Street { get; set; } = default;

    /// IsArchival
    [StswExcelAttribute("Is archival", "{0:YES;1;NO}")]
    public bool IsArchival { get; set; } = default;

    /// CreateDT
    [StswExcelAttribute("Create date", "yyyy-MM-dd")]
    public DateTime CreateDT { get; set; } = default;

    /// ShowDetails
    public bool ShowDetails { get; set; } = default;
}
