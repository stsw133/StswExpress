namespace TestConsole;
public class ContractorModel
{
    public int Id { get; set; }
    public string? Code { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public bool IsArchival { get; set; }
    public AddressModel? Address { get; set; }
}
