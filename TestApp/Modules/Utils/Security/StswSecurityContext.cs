namespace TestApp;
public partial class StswSecurityContext : StswObservableObject
{
    [StswObservableProperty] string _key = string.Empty;
    partial void OnKeyChanged(string oldValue, string newValue) => StswSecurity.Key = newValue;

    [StswObservableProperty] string? _inputGetHashString;
    partial void OnInputGetHashStringChanged(string? oldValue, string? newValue) => OutputGetHashString = newValue == null ? null : StswSecurity.GetHashString(newValue);

    [StswObservableProperty] string? _outputGetHashString;

    [StswObservableProperty] string? _inputEncrypt;
    partial void OnInputEncryptChanged(string? oldValue, string? newValue) => OutputEncrypt = newValue == null ? null : StswSecurity.Encrypt(newValue);

    [StswObservableProperty] string? _outputEncrypt;

    [StswObservableProperty] string? _inputDecrypt;
    partial void OnInputDecryptChanged(string? oldValue, string? newValue) => OutputDecrypt = newValue == null ? null : StswSecurity.Decrypt(newValue);

    [StswObservableProperty] string? _outputDecrypt;
}
