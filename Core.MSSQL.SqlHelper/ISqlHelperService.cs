using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace Core.MSSQL.SqlHelper
{
    public interface ISqlHelperService
    {
        // ---- Synchronous ----

        DataTable GetDataTableBySP(string spName);
        DataTable GetDataTableBySPWithParam(string spName, params object[] parameterValues);
        DataTable GetDataTableBySPWithParam(string spName, params SqlParameter[] parameters);
        DataTable GetDataTableBySql(string sql);
        DataTable GetDataTableBySql(string sql, params SqlParameter[] parameters);
        object GetScalarValueBySP(string spName);
        object GetScalarValueBySP(string spName, params SqlParameter[] parameters);

        /// <summary>Executes a T-SQL statement and returns the number of rows affected.</summary>
        int ExecuteSQL(string sql);
        /// <summary>Executes a parameterized T-SQL statement and returns the number of rows affected.</summary>
        int ExecuteSQL(string sql, params SqlParameter[] parameters);

        /// <summary>Returns true if <paramref name="tableName"/> has a row where <paramref name="column"/> equals <paramref name="value"/>.</summary>
        bool IsExist(string tableName, string column, string value);
        /// <summary>Returns true if <paramref name="tableName"/> has a row where <paramref name="column"/> equals <paramref name="value"/>.</summary>
        bool IsExist(string tableName, string column, int value);
        /// <summary>
        /// Returns true if <paramref name="tableName"/> has a row matching <paramref name="whereClause"/>.
        /// Use parameter placeholders (e.g. <c>Email = @email</c>) and pass values in <paramref name="parameters"/>.
        /// </summary>
        bool IsExist(string tableName, string whereClause, params SqlParameter[] parameters);

        // ---- Asynchronous ----

        Task<DataTable> GetDataTableBySPAsync(string spName, SqlParameter[] parameters = null, CancellationToken cancellationToken = default);
        Task<DataTable> GetDataTableBySqlAsync(string sql, SqlParameter[] parameters = null, CancellationToken cancellationToken = default);
        Task<object> GetScalarValueBySPAsync(string spName, SqlParameter[] parameters = null, CancellationToken cancellationToken = default);
        Task<object> GetScalarValueBySqlAsync(string sql, SqlParameter[] parameters = null, CancellationToken cancellationToken = default);
        Task<int> ExecuteSQLAsync(string sql, SqlParameter[] parameters = null, CancellationToken cancellationToken = default);
        Task<int> ExecuteSPAsync(string spName, SqlParameter[] parameters = null, CancellationToken cancellationToken = default);
        Task<bool> IsExistAsync(string tableName, string column, object value, CancellationToken cancellationToken = default);
        Task<bool> IsExistAsync(string tableName, string whereClause, SqlParameter[] parameters, CancellationToken cancellationToken = default);
    }
}
