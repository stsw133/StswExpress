namespace TestConsole;
public class DocumentPositionModel
{
    public int HeadId { get; set; }
    public int Id { get; set; }
    public int ArticleId { get; set; }
    public string? ArticleCode { get; set; }
    public string? ArticleName { get; set; }
    public decimal Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal Total => Quantity * Price;
    public string? Unit { get; set; }
}
