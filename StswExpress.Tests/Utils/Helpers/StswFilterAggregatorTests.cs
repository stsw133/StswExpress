using System;

namespace StswExpress.Tests;
public class StswFilterAggregatorTests
{
    [Fact]
    public void RegisterFilter_AddsAndRemovesFilters_Correctly()
    {
        var aggregator = new StswExpress.StswFilterAggregator();
        object key = new();

        aggregator.RegisterFilter(key, x => true);
        Assert.True(aggregator.HasFilters);

        aggregator.RegisterFilter(key, null);
        Assert.False(aggregator.HasFilters);
    }

    [Fact]
    public void CombinedFilter_NoFilters_ReturnsTrue()
    {
        var aggregator = new StswExpress.StswFilterAggregator();
        Assert.True(aggregator.CombinedFilter(new object()));
    }

    [Fact]
    public void CombinedFilter_AllFiltersTrue_ReturnsTrue()
    {
        var aggregator = new StswExpress.StswFilterAggregator();
        aggregator.RegisterFilter("f1", x => true);
        aggregator.RegisterFilter("f2", x => true);

        Assert.True(aggregator.CombinedFilter(new object()));
    }

    [Fact]
    public void CombinedFilter_AnyFilterFalse_ReturnsFalse()
    {
        var aggregator = new StswExpress.StswFilterAggregator();
        aggregator.RegisterFilter("f1", x => true);
        aggregator.RegisterFilter("f2", x => false);

        Assert.False(aggregator.CombinedFilter(new object()));
    }

    [Fact]
    public void CombinedFilter_NullFilter_Skipped()
    {
        var aggregator = new StswExpress.StswFilterAggregator();
        aggregator.RegisterFilter("f1", null);
        aggregator.RegisterFilter("f2", x => true);

        Assert.True(aggregator.CombinedFilter(new object()));
    }

    [Fact]
    public void HasFilters_IndicatesPresenceOfFilters()
    {
        var aggregator = new StswExpress.StswFilterAggregator();
        Assert.False(aggregator.HasFilters);

        aggregator.RegisterFilter("f1", x => true);
        Assert.True(aggregator.HasFilters);

        aggregator.RegisterFilter("f1", null);
        Assert.False(aggregator.HasFilters);
    }
}
