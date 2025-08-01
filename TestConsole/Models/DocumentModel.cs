namespace TestConsole;
public class DocumentModel
{
    public int Id { get; set; }
    public string Number { get; set; } = string.Empty;
    public DateTime IssueDT { get; set; }
    public int ContractorId { get; set; }
    public string? ContractorCode { get; set; }
    public string? ContractorName { get; set; }
    public IEnumerable<DocumentPositionModel> Positions { get; set; } = [];
}
