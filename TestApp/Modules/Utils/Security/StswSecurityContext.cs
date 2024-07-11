namespace TestApp;
public class StswSecurityContext : StswObservableObject
{
    /// Key
    public string Key
    {
        get => _key;
        set => SetProperty(ref _key, value, () => StswSecurity.Key = value);
    }
    private string _key = string.Empty;

    /// GetHashString
    public string? InputGetHashString
    {
        get => _inputGetHashString;
        set => SetProperty(ref _inputGetHashString, value, () => OutputGetHashString = StswSecurity.GetHashString(value));
    }
    private string? _inputGetHashString;

    public string? OutputGetHashString
    {
        get => _outputGetHashString;
        set => SetProperty(ref _outputGetHashString, value);
    }
    private string? _outputGetHashString;

    /// Encrypt
    public string? InputEncrypt
    {
        get => _inputEncrypt;
        set => SetProperty(ref _inputEncrypt, value, () => OutputEncrypt = StswSecurity.Encrypt(value));
    }
    private string? _inputEncrypt;

    public string? OutputEncrypt
    {
        get => _outputEncrypt;
        set => SetProperty(ref _outputEncrypt, value);
    }
    private string? _outputEncrypt;

    /// Decrypt
    public string? InputDecrypt
    {
        get => _inputDecrypt;
        set => SetProperty(ref _inputDecrypt, value, () => OutputDecrypt = StswSecurity.Decrypt(value));
    }
    private string? _inputDecrypt;

    public string? OutputDecrypt
    {
        get => _outputDecrypt;
        set => SetProperty(ref _outputDecrypt, value);
    }
    private string? _outputDecrypt;
}
