using System.ComponentModel;

namespace TestApp;

public enum ArticleType
{
    [Description("Commodity")]
    Commodity,
    [Description("Product")]
    Product,
    [Description("Service")]
    Service
}

public enum ContractorType
{
    [Description("Supplier")]
    Supplier,
    [Description("Recipient")]
    Recipient
}

public enum DiscountType
{
    [Description("Percentage")]
    Percentage,
    [Description("Value")]
    Value
}
