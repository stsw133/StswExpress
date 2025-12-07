using System.Data;

namespace StswExpress.Commons.Tests.Utils.Misc;
public class StswMappingTests
{
    [Fact]
    public void MapTo_SimpleType_ReturnsValues()
    {
        var dt = new DataTable();
        dt.Columns.Add("Value", typeof(int));
        dt.Rows.Add(1);
        dt.Rows.Add(2);

        var result = dt.MapTo<int>();
        Assert.Equal([1, 2], result);
    }

    [Fact]
    public void MapTo_KeyValuePair_ReturnsPairs()
    {
        var dt = new DataTable();
        dt.Columns.Add("Key", typeof(int));
        dt.Columns.Add("Value", typeof(string));
        dt.Rows.Add(1, "One");
        dt.Rows.Add(2, "Two");

        var result = dt.MapTo<KeyValuePair<int, string>>().ToList();

        Assert.Equal(new KeyValuePair<int, string>(1, "One"), result[0]);
        Assert.Equal(new KeyValuePair<int, string>(2, "Two"), result[1]);
    }

    [Fact]
    public void MapTo_Tuple_ReturnsTuples()
    {
        var dt = new DataTable();
        dt.Columns.Add("Id", typeof(int));
        dt.Columns.Add("Name", typeof(string));
        dt.Rows.Add(1, "One");
        dt.Rows.Add(2, "Two");

        var result = dt.MapTo<(int, string)>().ToList();

        Assert.Equal((1, "One"), result[0]);
        Assert.Equal((2, "Two"), result[1]);
    }
}
