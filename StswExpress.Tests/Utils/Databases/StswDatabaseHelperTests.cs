using Microsoft.Data.SqlClient;
using System.Data;

namespace StswExpress.Commons.Tests.Utils.Databases;
public class StswDatabaseHelperTests
{
    [Fact]
    public void GetOpened_OpensConnectionIfClosed()
    {
        var conn = new SqlConnection("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=master;Integrated Security=True;");
        Assert.Equal(ConnectionState.Closed, conn.State);
        var opened = conn.GetOpened();
        Assert.Equal(ConnectionState.Open, opened.State);
        conn.Close();
    }

    [Fact]
    public void LessSpaceQuery_RemovesExtraSpacesOutsideQuotes()
    {
        var query = "SELECT   *   FROM   Table WHERE Name = 'A   B'";
        var result = StswDatabaseHelper.LessSpaceQuery(query);
        Assert.Contains("'A   B'", result);
        Assert.DoesNotContain("  ", result.Replace("'A   B'", ""));
    }

    [Fact]
    public void PrepareInsertQuery_ThrowsIfNoInsertableProperties()
    {
        Assert.Throws<InvalidOperationException>(() =>
            StswDatabaseHelper.PrepareInsertQuery(new List<object>(), "Table", false));
    }

    [Fact]
    public void PrepareUpdateQuery_ThrowsIfWhereClauseEmpty()
    {
        Assert.Throws<ArgumentException>(() =>
            StswDatabaseHelper.PrepareUpdateQuery(new List<object>(), "Table", ""));
    }

    [Fact]
    public void PrepareUpdateQuery_ThrowsIfNoUpdatableColumns()
    {
        var where = "@Id";
        Assert.Throws<InvalidOperationException>(() =>
            StswDatabaseHelper.PrepareUpdateQuery(new List<TestModel>(), "Table", where));
    }

    [Fact]
    public void TrimTrailingDigits_RemovesDigitsFromEnd()
    {
        var name = "Column123";
        var trimmed = typeof(StswDatabaseHelper)
            .GetMethod("TrimTrailingDigits", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
            .Invoke(null, new object[] { name }) as string;
        Assert.Equal("Column", trimmed);
    }

    [Fact]
    public void GetSqlType_ReturnsCorrectSqlType()
    {
        var method = typeof(StswDatabaseHelper)
            .GetMethod("GetSqlType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!;
        Assert.Equal("NVARCHAR(MAX)", method.Invoke(null, new object[] { typeof(string) }));
        Assert.Equal("INT", method.Invoke(null, new object[] { typeof(int) }));
        Assert.Equal("BIGINT", method.Invoke(null, new object[] { typeof(long) }));
        Assert.Equal("DECIMAL(18, 2)", method.Invoke(null, new object[] { typeof(decimal) }));
        Assert.Equal("FLOAT", method.Invoke(null, new object[] { typeof(double) }));
        Assert.Equal("BIT", method.Invoke(null, new object[] { typeof(bool) }));
        Assert.Equal("DATETIME", method.Invoke(null, new object[] { typeof(DateTime) }));
        Assert.Throws<NotSupportedException>(() => method.Invoke(null, new object[] { typeof(object) }));
    }

    [Fact]
    public void ParametersAddList_ThrowsIfListTooLarge()
    {
        var cmd = new SqlCommand("SELECT * FROM Table WHERE Id IN (@Ids)");
        var list = Enumerable.Range(1, 21).ToList();
        Assert.Throws<ArgumentException>(() => cmd.ParametersAddList("@Ids", list));
    }

    [Fact]
    public void ParametersAddList_ReplacesParameterWithNullIfListEmpty()
    {
        var cmd = new SqlCommand("SELECT * FROM Table WHERE Id IN (@Ids)");
        cmd.ParametersAddList("@Ids", new List<int>());
        Assert.Contains("NULL", cmd.CommandText);
    }

    [Fact]
    public void PrepareCommand_AddsParametersFromDictionary()
    {
        var cmd = new SqlCommand("SELECT * FROM Table WHERE Id = @Id AND Name = @Name");
        var dict = new Dictionary<string, object?> { { "Id", 1 }, { "Name", "Test" } };
        var result = typeof(StswDatabaseHelper)
            .GetMethod("PrepareCommand", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
            .Invoke(null, new object[] { cmd, dict, false }) as SqlCommand;
        Assert.Equal(2, result!.Parameters.Count);
        Assert.Equal(1, result.Parameters["@Id"].Value);
        Assert.Equal("Test", result.Parameters["@Name"].Value);
    }

    [Fact]
    public void PrepareParameter_AddsByteArrayParameter()
    {
        var cmd = new SqlCommand();
        var method = typeof(StswDatabaseHelper)
            .GetMethod("PrepareParameter", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!;
        var bytes = new byte[] { 1, 2, 3 };
        method.Invoke(null, new object[] { cmd, "@Data", bytes });
        Assert.True(cmd.Parameters.Contains("@Data"));
        Assert.Equal(bytes, cmd.Parameters["@Data"].Value);
    }

    [Fact]
    public void GetWritableScalarPropertyNames_ExcludesNonWritableAndCollections()
    {
        var method = typeof(StswDatabaseHelper)
            .GetMethod("GetWritableScalarPropertyNames", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!;
        var names = method.Invoke(null, new object[] { typeof(TestModel) }) as List<string>;
        Assert.Contains("Id", names!);
        Assert.Contains("Name", names!);
        Assert.DoesNotContain("Items", names!);
    }

    private class TestModel : StswObservableObject, IStswCollectionItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public List<string> Items { get; set; } = new();
        public StswItemState ItemState { get; set; }
        public bool? ShowDetails { get; set; }
    }
}
