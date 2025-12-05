namespace StswExpress.Core;

/// <summary>
/// Shared contract for StswExpress buttons across UI frameworks.
/// </summary>
/// <typeparam name="TRadius">Type used to describe rounded corner settings in a given UI framework.</typeparam>
public interface IStswButton<TRadius>
{
    /// <summary>
    /// Gets or sets a value indicating whether the control should clip its contents to the configured corner radius.
    /// </summary>
    bool CornerClipping { get; set; }

    /// <summary>
    /// Gets or sets the corner radius value expressed using the framework's specific corner representation.
    /// </summary>
    TRadius CornerRadius { get; set; }
}
