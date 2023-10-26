using System.ComponentModel;

namespace TestApp;

public enum ContractorType
{
    [Description("Supplier")]
    Supplier,
    [Description("Recipient")]
    Recipient
}
