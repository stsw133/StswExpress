using System;
using System.Collections.Generic;

namespace StswExpress;

/// <summary>
/// Internal aggregator to combine multiple filters from external controls.
/// </summary>
[Stsw("0.16.0")]
public class StswFilterAggregator
{
    private readonly Dictionary<object, Predicate<object>> _registeredFilters = [];

    /// <summary>
    /// Registers or updates a filter by a given key. Passing null removes the filter.
    /// </summary>
    public void RegisterFilter(object key, Predicate<object>? filter)
    {
        if (filter == null)
            _registeredFilters.Remove(key);
        else
            _registeredFilters[key] = filter;
    }

    /// <summary>
    /// Applies the combined filters in an AND fashion. Returns false if any filter fails.
    /// </summary>
    public bool CombinedFilter(object item)
    {
        if (_registeredFilters.Count == 0)
            return true;

        foreach (var filter in _registeredFilters.Values)
        {
            if (filter == null)
                continue;

            if (!filter(item))
                return false;
        }
        return true;
    }

    /// <summary>
    /// Indicates whether there are any registered filters.
    /// </summary>
    public bool HasFilters => _registeredFilters.Count > 0;
}
