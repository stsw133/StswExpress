using System;

namespace TestApp.Modules.Contractors;

public class ContractorModel
{
    /// IDs
    public int ID { get; set; } = default;
    public string? Name { get; set; } = default;

    /// Data
    public string? Type { get; set; } = default;
    public byte[]? Icon { get; set; } = default;
    
    /// States
    public bool IsArchival { get; set; } = default;

    /// Logs
    public DateTime CreateDT { get; set; } = default;
}
