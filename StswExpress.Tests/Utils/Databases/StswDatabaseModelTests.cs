namespace StswExpress.Commons.Tests;
public class StswDatabaseModelTests
{
    [Fact]
    public void Constructor_Default_SetsDefaults()
    {
        var model = new StswDatabaseModel();
        Assert.Equal(StswDatabaseType.MSSQL, model.Type);
        Assert.Null(model.Server);
        Assert.Null(model.Database);
        Assert.Null(model.Login);
        Assert.Null(model.Password);
    }

    [Fact]
    public void Constructor_WithParameters_SetsProperties()
    {
        var model = new StswDatabaseModel("srv", 1234, "db", "user", "pass");
        Assert.Equal("srv", model.Server);
        Assert.Equal(1234, model.Port);
        Assert.Equal("db", model.Database);
        Assert.Equal("user", model.Login);
        Assert.Equal("pass", model.Password);
    }

    [Fact]
    public void GetConnString_ThrowsIfServerOrDatabaseMissing()
    {
        var model = new StswDatabaseModel();
        Assert.Throws<InvalidOperationException>(() => model.GetConnString());

        model.Server = "srv";
        Assert.Throws<InvalidOperationException>(() => model.GetConnString());

        model.Server = null;
        model.Database = "db";
        Assert.Throws<InvalidOperationException>(() => model.GetConnString());
    }

    [Fact]
    public void GetConnString_ThrowsIfLoginOrPasswordMissing_WhenNotIntegratedSecurity()
    {
        var model = new StswDatabaseModel("srv", "db");
        model.UseIntegratedSecurity = false;
        Assert.Throws<InvalidOperationException>(() => model.GetConnString());

        model.Login = "user";
        Assert.Throws<InvalidOperationException>(() => model.GetConnString());

        model.Login = null;
        model.Password = "pass";
        Assert.Throws<InvalidOperationException>(() => model.GetConnString());
    }

    [Fact]
    public void GetConnString_MSSQL_IntegratedSecurity()
    {
        var model = new StswDatabaseModel("srv", 1433, "db");
        model.Type = StswDatabaseType.MSSQL;
        model.UseIntegratedSecurity = true;
        model.Encrypt = true;
        var connStr = model.GetConnString();
        Assert.Contains("Server=srv,1433", connStr);
        Assert.Contains("Database=db", connStr);
        Assert.Contains("Integrated Security=True", connStr);
        Assert.Contains("Encrypt=True", connStr);
        Assert.Contains("Application Name=", connStr);
    }

    [Fact]
    public void GetConnString_MSSQL_UsernamePassword()
    {
        var model = new StswDatabaseModel("srv", 1433, "db", "user", "pass");
        model.Type = StswDatabaseType.MSSQL;
        model.UseIntegratedSecurity = false;
        model.Encrypt = false;
        var connStr = model.GetConnString();
        Assert.Contains("Server=srv,1433", connStr);
        Assert.Contains("Database=db", connStr);
        Assert.Contains("User Id=user", connStr);
        Assert.Contains("Password=pass", connStr);
        Assert.Contains("Encrypt=False", connStr);
        Assert.Contains("Application Name=", connStr);
    }

    [Fact]
    public void GetConnString_MySQL_ThrowsIfIntegratedSecurity()
    {
        var model = new StswDatabaseModel("srv", 3306, "db", "user", "pass");
        model.Type = StswDatabaseType.MySQL;
        model.UseIntegratedSecurity = true;
        Assert.Throws<NotSupportedException>(() => model.GetConnString());
    }

    [Fact]
    public void GetConnString_MySQL_UsernamePassword()
    {
        var model = new StswDatabaseModel("srv", 3306, "db", "user", "pass");
        model.Type = StswDatabaseType.MySQL;
        model.UseIntegratedSecurity = false;
        model.Encrypt = true;
        var connStr = model.GetConnString();
        Assert.Contains("Server=srv;Port=3306", connStr);
        Assert.Contains("Database=db", connStr);
        Assert.Contains("Uid=user", connStr);
        Assert.Contains("Pwd=pass", connStr);
        Assert.Contains("Encrypt=True", connStr);
        Assert.Contains("Application Name=", connStr);
    }

    [Fact]
    public void GetConnString_PostgreSQL_ThrowsIfIntegratedSecurity()
    {
        var model = new StswDatabaseModel("srv", 5432, "db", "user", "pass");
        model.Type = StswDatabaseType.PostgreSQL;
        model.UseIntegratedSecurity = true;
        Assert.Throws<NotSupportedException>(() => model.GetConnString());
    }

    [Fact]
    public void GetConnString_PostgreSQL_UsernamePassword()
    {
        var model = new StswDatabaseModel("srv", 5432, "db", "user", "pass");
        model.Type = StswDatabaseType.PostgreSQL;
        model.UseIntegratedSecurity = false;
        model.Encrypt = false;
        var connStr = model.GetConnString();
        Assert.Contains("Host=srv;Port=5432", connStr);
        Assert.Contains("Database=db", connStr);
        Assert.Contains("User Id=user", connStr);
        Assert.Contains("Password=pass", connStr);
        Assert.Contains("Encrypt=False", connStr);
        Assert.Contains("Application Name=", connStr);
    }

    [Fact]
    public void GetConnString_ThrowsIfUnknownType()
    {
        var model = new StswDatabaseModel("srv", "db", "user", "pass");
        model.Type = (StswDatabaseType)999;
        model.UseIntegratedSecurity = false;
        Assert.Throws<NotSupportedException>(() => model.GetConnString());
    }

    [Fact]
    public void Property_Setters_RaisePropertyChanged()
    {
        var model = new StswDatabaseModel();
        bool changed = false;
        model.PropertyChanged += (s, e) => changed = true;
        model.Name = "Test";
        Assert.True(changed);
    }
}
