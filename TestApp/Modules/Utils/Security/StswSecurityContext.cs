namespace TestApp;
public partial class StswSecurityContext : StswObservableObject
{
    /// Key
    //public string Key
    //{
    //    get => _key;
    //    set => SetProperty(ref _key, value, () => StswSecurity.Key = value);
    //}
    [StswObservableProperty(CallbackMethod = "() => StswSecurity.Key = value")] string _key = string.Empty;

    //public string? InputGetHashString
    //{
    //    get => _inputGetHashString;
    //    set => SetProperty(ref _inputGetHashString, value, () => OutputGetHashString = value == null ? null : StswSecurity.GetHashString(value));
    //}
    [StswObservableProperty(CallbackMethod = "() => OutputGetHashString = value == null ? null : StswSecurity.GetHashString(value)")] string? _inputGetHashString;

    [StswObservableProperty] string? _outputGetHashString;

    //public string? InputEncrypt
    //{
    //    get => _inputEncrypt;
    //    set => SetProperty(ref _inputEncrypt, value, () => OutputEncrypt = value == null ? null : StswSecurity.Encrypt(value));
    //}
    [StswObservableProperty(CallbackMethod = "() => OutputEncrypt = value == null ? null : StswSecurity.Encrypt(value)")] string? _inputEncrypt;

    [StswObservableProperty] string? _outputEncrypt;

    //public string? InputDecrypt
    //{
    //    get => _inputDecrypt;
    //    set => SetProperty(ref _inputDecrypt, value, () => OutputDecrypt = value == null ? null : StswSecurity.Decrypt(value));
    //}
    [StswObservableProperty(CallbackMethod = "() => OutputDecrypt = value == null ? null : StswSecurity.Decrypt(value)")] string? _inputDecrypt;

    [StswObservableProperty] string? _outputDecrypt;
}
