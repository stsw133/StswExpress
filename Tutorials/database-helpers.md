# StswDatabaseHelper SQL Helpers Tutorial (StswExpress.Commons)

This tutorial shows how to **correctly use SQL helpers from `StswDatabaseHelper`** in `StswExpress.Commons`, with practical examples focused on:

- `Get`
- `ExecuteNonQuery`
- `ExecuteScalar`
- `TempTableInsert` + `Get`

The examples target SQL Server (`Microsoft.Data.SqlClient`) and are written in C#.

---

## 1. Prerequisites

Install/use:

- `StswExpress.Commons`
- `Microsoft.Data.SqlClient`

Typical namespace import:

```csharp
using Microsoft.Data.SqlClient;
using StswExpress.Commons;
```

Prepare a `StswDatabaseModel` and (optionally) set a default timeout:

```csharp
var db = new StswDatabaseModel
{
    Type = StswDatabaseType.MSSQL,
    Server = "localhost",
    Database = "MyDatabase",
    UseIntegratedSecurity = true,
    DefaultTimeout = 30
};
```

> `StswDatabaseHelper` methods are extension methods. You can call them from:
>
> - `SqlConnection`
> - `SqlTransaction`
> - `StswDatabaseModel`

---

## 2. Quick rules for correct usage

1. **Always parameterize SQL** (`@Id`, `@Status`, etc.) — never concatenate untrusted input.
2. **Prefer `StswDatabaseModel` overloads** when you want automatic connection creation and model-level default timeout.
3. **Use transactions** when multiple operations must succeed/fail together.
4. **When using list parameters in `IN (...)`, keep lists small** (helper supports up to 20 items in a single list parameter replacement).
5. **`TempTableInsert` creates `#temp` table automatically** (it prepends `#` if needed), so keep subsequent reads in the **same connection/transaction scope**.
6. If `StswDatabases.Config.IsEnabled == false`, helper methods short-circuit and return default/empty values.

---

## 3. `Get` — reading rows into models

### 3.1 Basic typed read

```csharp
public sealed class UserDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

var users = db.Get<UserDto>(
    @"SELECT Id, Name, IsActive
      FROM dbo.Users
      WHERE IsActive = @IsActive",
    new { IsActive = true }
).ToList();
```

Use this when query columns map 1:1 to DTO properties.

### 3.2 Dynamic type overload (`Type`)

```csharp
IEnumerable<object?> raw = db.Get(
    typeof(UserDto),
    "SELECT Id, Name, IsActive FROM dbo.Users WHERE Id = @Id",
    new { Id = 10 }
);

var single = raw.Cast<UserDto>().FirstOrDefault();
```

Useful when result type is known at runtime only.

### 3.3 Read with list parameter (`IN`)

```csharp
var ids = new List<int> { 1, 2, 3 };

var selected = db.Get<UserDto>(
    "SELECT Id, Name, IsActive FROM dbo.Users WHERE Id IN (@Ids)",
    new { Ids = ids }
).ToList();
```

`StswDatabaseHelper` expands list values into separate SQL parameters under the hood.

---

## 4. `ExecuteNonQuery` — INSERT / UPDATE / DELETE

### 4.1 Single UPDATE

```csharp
var affected = db.ExecuteNonQuery(
    @"UPDATE dbo.Users
      SET IsActive = @IsActive
      WHERE Id = @Id",
    new { IsActive = false, Id = 10 }
);

Console.WriteLine($"Rows affected: {affected}");
```

### 4.2 Batch execution with multiple parameter objects

If you pass an `IEnumerable<object>` as `parameters`, helper executes the same statement multiple times.

```csharp
var batch = new[]
{
    new { Id = 1, IsActive = false },
    new { Id = 2, IsActive = false },
    new { Id = 3, IsActive = false }
};

var totalAffected = db.ExecuteNonQuery(
    "UPDATE dbo.Users SET IsActive = @IsActive WHERE Id = @Id",
    batch
);
```

### 4.3 Inside transaction

```csharp
using var conn = db.OpenedConnection();
using var tran = conn.BeginTransaction();

try
{
    conn.ExecuteNonQuery(
        "UPDATE dbo.Accounts SET Balance = Balance - @Amount WHERE Id = @FromId",
        new { Amount = 100m, FromId = 1 },
        sqlTran: tran
    );

    conn.ExecuteNonQuery(
        "UPDATE dbo.Accounts SET Balance = Balance + @Amount WHERE Id = @ToId",
        new { Amount = 100m, ToId = 2 },
        sqlTran: tran
    );

    tran.Commit();
}
catch
{
    tran.Rollback();
    throw;
}
```

---

## 5. `ExecuteScalar` — single value result

### 5.1 Count rows

```csharp
int activeUsers = db.ExecuteScalar<int>(
    "SELECT COUNT(1) FROM dbo.Users WHERE IsActive = @IsActive",
    new { IsActive = true }
);
```

### 5.2 Last identity-like read

```csharp
int? maxId = db.ExecuteScalar<int?>(
    "SELECT MAX(Id) FROM dbo.Users"
);
```

### 5.3 Existence check

```csharp
bool exists = db.ExecuteScalar<int>(
    "SELECT CASE WHEN EXISTS(SELECT 1 FROM dbo.Users WHERE Email = @Email) THEN 1 ELSE 0 END",
    new { Email = "john@example.com" }
) == 1;
```

---

## 6. `TempTableInsert` + `Get` (recommended for larger filter sets)

This pattern is very useful when you need to send many IDs/rows to SQL and join them efficiently.

### 6.1 Example: pass selected IDs via temp table and fetch matching users

```csharp
public sealed class IdRow
{
    public int Id { get; set; }
}

var idRows = new[]
{
    new IdRow { Id = 101 },
    new IdRow { Id = 205 },
    new IdRow { Id = 333 }
};

using var conn = db.OpenedConnection();
using var tran = conn.BeginTransaction();

try
{
    // Creates #FilterIds and bulk-inserts rows.
    conn.TempTableInsert(idRows, "FilterIds", sqlTran: tran);

    // IMPORTANT: query in same connection/transaction scope, because #temp is session-scoped.
    var result = conn.Get<UserDto>(
        @"SELECT u.Id, u.Name, u.IsActive
          FROM dbo.Users u
          INNER JOIN #FilterIds f ON f.Id = u.Id",
        sqlTran: tran
    ).ToList();

    tran.Commit();
}
catch
{
    tran.Rollback();
    throw;
}
```

### 6.2 Dictionary/object-based temp insert

`TempTableInsert` also supports non-generic `IEnumerable` (e.g. list of dictionaries/anonymous-like merged structures), as long as columns can be inferred.

```csharp
var rows = new List<Dictionary<string, object?>>
{
    new() { ["Id"] = 1, ["Tag"] = "A" },
    new() { ["Id"] = 2, ["Tag"] = "B" }
};

using var conn = db.OpenedConnection();
using var tran = conn.BeginTransaction();

conn.TempTableInsert(rows, "MyTemp", sqlTran: tran);

var readBack = conn.Get<TempRowDto>(
    "SELECT Id, Tag FROM #MyTemp",
    sqlTran: tran
).ToList();

tran.Commit();
```

---

## 7. Common pitfalls and how to avoid them

1. **Trying to read `#temp` after connection closes**  
   Temp table disappears with session end. Keep `TempTableInsert` and `Get` in the same open connection (usually one transaction block).

2. **Too many values in one list parameter**  
   List replacement in `IN (@Ids)` has a size guard (20). For bigger datasets, prefer `TempTableInsert` and join.

3. **Forgetting `ToList()` before disposing scope**  
   Materialize results before leaving `using` blocks when needed later.

4. **Relying on execution while global DB helper is disabled**  
   If `StswDatabases.Config.IsEnabled` is off, calls return defaults/empty collections.

5. **Mixing different timeout expectations**  
   `StswDatabaseModel` overloads use `model.DefaultTimeout ?? timeout` behavior. Set `DefaultTimeout` explicitly for consistency.

---

## 8. Minimal end-to-end sample

```csharp
var db = new StswDatabaseModel
{
    Type = StswDatabaseType.MSSQL,
    Server = "localhost",
    Database = "MyDatabase",
    UseIntegratedSecurity = true,
    DefaultTimeout = 30
};

using var conn = db.OpenedConnection();
using var tran = conn.BeginTransaction();

try
{
    conn.ExecuteNonQuery(
        "UPDATE dbo.Users SET IsActive = @Flag WHERE Id = @Id",
        new { Flag = true, Id = 42 },
        sqlTran: tran
    );

    int total = conn.ExecuteScalar<int>(
        "SELECT COUNT(1) FROM dbo.Users WHERE IsActive = 1",
        sqlTran: tran
    );

    var filter = new[] { new { Id = 42 }, new { Id = 43 } };
    conn.TempTableInsert(filter, "FilterUsers", sqlTran: tran);

    var rows = conn.Get<UserDto>(
        @"SELECT u.Id, u.Name, u.IsActive
          FROM dbo.Users u
          INNER JOIN #FilterUsers f ON f.Id = u.Id",
        sqlTran: tran
    ).ToList();

    tran.Commit();
}
catch
{
    tran.Rollback();
    throw;
}
```

---

## 9. When to choose which method

- Use **`Get<T>`** for result sets mapped to models.
- Use **`ExecuteNonQuery`** for writes (insert/update/delete) where affected row count matters.
- Use **`ExecuteScalar<T>`** for one value (count, max, existence flags, computed scalar).
- Use **`TempTableInsert` + `Get`** for larger, structured filters or join-heavy workflows.

If you follow these patterns, your code will stay clear, safe (parameterized), and performant for typical SQL Server operations with `StswExpress.Commons`.
