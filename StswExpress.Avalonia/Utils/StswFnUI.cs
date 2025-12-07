namespace StswExpress.Avalonia;
/// <summary>
/// Utility class providing various helper functions for general use.
/// </summary>
public static class StswFnUI
{
    #region Assembly functions
    /// <summary>
    /// Gets the name and version number of the currently executing application for XAML bindings.
    /// </summary>
    /// <returns>A string containing the name and version number of the currently executing application.</returns>
    public static string? AppNameAndVersion => StswFn.AppVersion != "1" ? $"{StswFn.AppName} {StswFn.AppVersion}" : StswFn.AppName;
    #endregion

    #region Binding helpers
    /// <summary>
    /// Boolean value representing true for XAML bindings.
    /// </summary>
    public static readonly bool True = true;

    /// <summary>
    /// Boolean value representing false for XAML bindings.
    /// </summary>
    public static readonly bool False = false;

    /// <summary>
    /// Gets the current date with the time component set to 00:00:00.
    /// </summary>
    public static DateTime CurrentDate => DateTime.Today;

    /// <summary>
    /// Gets the current date and time.
    /// </summary>
    public static DateTime CurrentDateTime => DateTime.Now;
    #endregion
}
