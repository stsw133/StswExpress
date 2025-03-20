using System.ComponentModel;

namespace StswExpress.Commons;

/// <summary>
/// Provides configuration settings for managing database connections in the application.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public class StswDatabasesConfig
{
    /// <summary>
    /// Gets or sets a value indicating whether connections should be automatically disposed of after use.
    /// </summary>
    public bool AutoDisposeConnection { get; set; } = true;

    /// <summary>
    /// Gets or sets the delimiter used for mapping in database queries.
    /// </summary>
    public char DelimiterForMapping { get; set; } = '/';

    /// <summary>
    /// Gets or sets the location of the file where encrypted database connections are stored.
    /// </summary>
    public string FilePath { get; set; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "databases.stsw");

    /// <summary>
    /// Gets or sets a value indicating whether database connecting is globally enabled. 
    /// If set to <see langword="false"/>, all <see cref="StswDatabaseHelper"/>'s methods will return default value.
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether queries should be optimized to use less space.
    /// </summary>
    public bool MakeLessSpaceQuery { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to always return if the application is running in designer mode.
    /// </summary>
    public bool ReturnIfInDesignerMode { get; set; } = true;
}
