namespace StswExpress.Commons;

/// <summary>
/// Represents a standard error response for Web API calls.
/// </summary>
public class StswHttpErrorResponse
{
    /// <summary>
    /// Gets or sets the type of the error, typically a link to documentation about the error.
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// Gets or sets the title of the error, which is a short description of the error.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the HTTP status code associated with the error.
    /// </summary>
    public int? Status { get; set; }

    /// <summary>
    /// Gets or sets the detail of the error, which provides more specific information about what went wrong.
    /// </summary>
    public string? Detail { get; set; }

    /// <summary>
    /// Gets or sets the instance identifier of the error, which can be used to track a specific occurrence of the error.
    /// </summary>
    public string? Instance { get; set; }
}
