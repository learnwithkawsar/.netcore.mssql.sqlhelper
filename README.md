# Core.MSSQL.SqlHelper

Simple helpers for common Microsoft SQL Server operations in .NET and ASP.NET Core:
run stored procedures or SQL and get back a `DataTable`, a scalar value or the number
of affected rows, synchronously or asynchronously.

Targets `netstandard2.0` and `net8.0`, and uses [`Microsoft.Data.SqlClient`](https://www.nuget.org/packages/Microsoft.Data.SqlClient).

## Install

```powershell
dotnet add package Core.MSSQL.SqlHelper
```

## Register

```csharp
// Program.cs
var connectionString = builder.Configuration.GetConnectionString("LocalDbConnection");
builder.Services.RegisterSqlHelper(connectionString);
```

## Use

```csharp
using Core.MSSQL.SqlHelper;
using Microsoft.Data.SqlClient;

public class DemoController : ControllerBase
{
    private readonly ISqlHelperService _sql;

    public DemoController(ISqlHelperService sql)
    {
        _sql = sql;
    }

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        // Stored procedure with parameters
        DataTable users = await _sql.GetDataTableBySPAsync(
            "dbo.GetUsers",
            new[] { new SqlParameter("@IsActive", true) },
            ct);

        // Parameterized SQL (never concatenate user input into SQL)
        int updated = await _sql.ExecuteSQLAsync(
            "UPDATE dbo.Users SET LastSeen = SYSUTCDATETIME() WHERE Id = @id",
            new[] { new SqlParameter("@id", 42) },
            ct);

        // Existence checks: table/column names are validated and quoted, the value is a parameter
        bool emailTaken = await _sql.IsExistAsync("dbo.Users", "Email", "a@example.com", ct);

        return Ok(new { count = users.Rows.Count, updated, emailTaken });
    }
}
```

Synchronous versions are also available: `GetDataTableBySP`, `GetDataTableBySPWithParam`,
`GetDataTableBySql`, `GetScalarValueBySP`, `ExecuteSQL` and `IsExist`.

For lower-level access (connections, transactions, `DataSet`, `SqlDataReader`, `XmlReader`),
use the static `SqlHelper` class directly.

### Command timeout

All commands use `SqlHelper.CommandTimeout` (in seconds). It defaults to `0` (no timeout)
for backward compatibility; setting a real value is recommended:

```csharp
SqlHelper.CommandTimeout = 60;
```

## Upgrading from 1.x

Version 2.0 contains breaking changes:

- **`System.Data.SqlClient` was replaced by `Microsoft.Data.SqlClient`.** Change
  `using System.Data.SqlClient;` to `using Microsoft.Data.SqlClient;` in your code.
- **`IsExist` is now safe from SQL injection.** Table and column names must be valid SQL
  identifiers (an `ArgumentException` is thrown otherwise), and values are sent as parameters.
- **`IsExist` no longer returns `true` when the query fails.** Database errors are now thrown.
- **`IsExist(table, whereClause)` now takes `params SqlParameter[]`.** Use placeholders such as
  `Email = @email` and pass the values as parameters. The where clause itself is still used as
  written, so never build it from user input.
- **`ExecuteSQL` returns `int`** (rows affected) instead of `dynamic`.
- **`RegisterSqlHelper` returns `IServiceCollection`** and also registers the concrete
  `SqlHelperService`.
- **`GetDataTableBySPWithParam(string, params SqlParameter[])`** now passes the parameters
  correctly (1.x passed them as parameter values).

## License

MIT
