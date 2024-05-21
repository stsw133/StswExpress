using System.ComponentModel;

namespace TestApp;

public enum ContractorType
{
    [Description("Supplier")]
    Supplier,
    [Description("Recipient")]
    Recipient
}

public enum ChangelogType
{
    [Description("Major")]
    Major,
    [Description("Minor")]
    Minor,
    [Description("Patch")]
    Patch
}
