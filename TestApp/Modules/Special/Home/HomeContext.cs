using System;
using System.Diagnostics;
using System.Reflection;

namespace TestApp;

public class HomeContext : StswObservableObject
{
    public string? Authors => FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).CompanyName;
    public string? Name => typeof(StswApp).Assembly.GetName().Name;
    public string? Version => typeof(StswApp).Assembly.GetName().Version?.ToString()?[..^2];
    public string? NetVersion => Environment.Version?.ToString();
}
