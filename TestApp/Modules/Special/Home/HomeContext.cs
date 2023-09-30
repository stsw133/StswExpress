using System.Diagnostics;
using System.Reflection;

namespace TestApp;

public class HomeContext : StswObservableObject
{
    public string? Author => FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).CompanyName;
    public string? Name => Assembly.GetExecutingAssembly().GetName().Name;
    public string? Version => $"{Assembly.GetExecutingAssembly().GetName().Version?.Major}.{Assembly.GetExecutingAssembly().GetName().Version?.Minor}";
}
