using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StswExpress;

/// <summary>
/// Data model for <see cref="StswDataGrid"/>'s filters.
/// </summary>
[Stsw(null, Changes = StswPlannedChanges.None)]
public class StswDataGridFiltersDataModel
{
    /// <summary>
    /// Gets or sets the action for applying the filters.
    /// </summary>
    public Action? Apply { get; internal set; }

    /// <summary>
    /// Gets or sets the action for clearing the filters.
    /// </summary>
    public Action? Clear { get; internal set; }

    /// <summary>
    /// Gets or sets the SQL filter.
    /// </summary>
    public string SqlFilter { get; internal set; } = "1=1";

    /// <summary>
    /// Gets or sets the list of SQL parameters.
    /// </summary>
    public IList<SqlParameter> SqlParameters { get; internal set; } = [];

    /// <summary>
    /// Gets or sets the list of SQL parameters.
    /// </summary>
    internal void MakeSqlParameters(IList<object> parameters) => SqlParameters = [.. parameters.Cast<SqlParameter>()];
}
