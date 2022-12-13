using System;

namespace TestApp.Modules.Main;

public class UserModel
{
    public int ID { get; set; } = default;
    public string? Type { get; set; } = default;
    public byte[]? Icon { get; set; } = default;
    public string? Name { get; set; } = default;
    public bool IsEnabled { get; set; } = default;
    public DateTime CreateDT { get; set; } = default;
}
