using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace StswExpress.Wpf;

/// <summary>
/// Data model for <see cref="StswDataGrid"/>'s filters.
/// </summary>
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
    public IList SqlParameters { get; internal set; } = Array.Empty<object>();

    /// <summary>
    /// Gets or sets the list of SQL parameters.
    /// </summary>
    internal void MakeSqlParameters(IList<object> parameters)
    {
        var parameterType = parameters.FirstOrDefault()?.GetType();
        if (parameterType == null)
        {
            SqlParameters = Array.Empty<object>();
            return;
        }

        var typedList = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(parameterType))!;
        foreach (var parameter in parameters.Where(x => x is not null))
            typedList.Add(parameter);

        SqlParameters = typedList;
    }
}
