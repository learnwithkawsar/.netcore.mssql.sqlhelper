using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace Core.MSSQL.SqlHelper
{
    public class SqlHelperService : ISqlHelperService
    {
        private readonly string _connectionString;

        public SqlHelperService(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString));
            }
            _connectionString = connectionString;
        }

        // ---------------------------------------------------------------- sync

        public int ExecuteSQL(string sql)
            => SqlHelper.ExecuteNonQuery(_connectionString, CommandType.Text, sql);

        public int ExecuteSQL(string sql, params SqlParameter[] parameters)
            => SqlHelper.ExecuteNonQuery(_connectionString, CommandType.Text, sql, parameters);

        public DataTable GetDataTableBySP(string spName)
            => SqlHelper.ExecuteDataTable(_connectionString, CommandType.StoredProcedure, spName);

        public DataTable GetDataTableBySPWithParam(string spName, params object[] parameterValues)
            => SqlHelper.ExecuteDataTable(_connectionString, spName, parameterValues);

        public DataTable GetDataTableBySPWithParam(string spName, params SqlParameter[] parameters)
            => SqlHelper.ExecuteDataTable(_connectionString, CommandType.StoredProcedure, spName, parameters);

        public DataTable GetDataTableBySql(string sql)
            => SqlHelper.ExecuteDataTable(_connectionString, CommandType.Text, sql);

        public DataTable GetDataTableBySql(string sql, params SqlParameter[] parameters)
            => SqlHelper.ExecuteDataTable(_connectionString, CommandType.Text, sql, parameters);

        public object GetScalarValueBySP(string spName)
            => SqlHelper.ExecuteScalar(_connectionString, CommandType.StoredProcedure, spName);

        public object GetScalarValueBySP(string spName, params SqlParameter[] parameters)
            => SqlHelper.ExecuteScalar(_connectionString, CommandType.StoredProcedure, spName, parameters);

        public bool IsExist(string tableName, string column, string value)
            => IsExistCore(tableName, column, value);

        public bool IsExist(string tableName, string column, int value)
            => IsExistCore(tableName, column, value);

        public bool IsExist(string tableName, string whereClause, params SqlParameter[] parameters)
        {
            var sql = BuildExistsQueryWithWhere(tableName, whereClause);
            return SqlHelper.ExecuteScalar(_connectionString, CommandType.Text, sql, parameters ?? new SqlParameter[0]) != null;
        }

        private bool IsExistCore(string tableName, string column, object value)
        {
            var sql = BuildExistsQuery(tableName, column);
            var parameter = new SqlParameter("@value", value ?? DBNull.Value);
            return SqlHelper.ExecuteScalar(_connectionString, CommandType.Text, sql, parameter) != null;
        }

        // --------------------------------------------------------------- async

        public async Task<DataTable> GetDataTableBySPAsync(string spName, SqlParameter[] parameters = null, CancellationToken cancellationToken = default)
            => await ExecuteDataTableAsync(CommandType.StoredProcedure, spName, parameters, cancellationToken).ConfigureAwait(false);

        public async Task<DataTable> GetDataTableBySqlAsync(string sql, SqlParameter[] parameters = null, CancellationToken cancellationToken = default)
            => await ExecuteDataTableAsync(CommandType.Text, sql, parameters, cancellationToken).ConfigureAwait(false);

        public Task<object> GetScalarValueBySPAsync(string spName, SqlParameter[] parameters = null, CancellationToken cancellationToken = default)
            => ExecuteScalarAsync(CommandType.StoredProcedure, spName, parameters, cancellationToken);

        public Task<object> GetScalarValueBySqlAsync(string sql, SqlParameter[] parameters = null, CancellationToken cancellationToken = default)
            => ExecuteScalarAsync(CommandType.Text, sql, parameters, cancellationToken);

        public Task<int> ExecuteSQLAsync(string sql, SqlParameter[] parameters = null, CancellationToken cancellationToken = default)
            => ExecuteNonQueryAsync(CommandType.Text, sql, parameters, cancellationToken);

        public Task<int> ExecuteSPAsync(string spName, SqlParameter[] parameters = null, CancellationToken cancellationToken = default)
            => ExecuteNonQueryAsync(CommandType.StoredProcedure, spName, parameters, cancellationToken);

        public async Task<bool> IsExistAsync(string tableName, string column, object value, CancellationToken cancellationToken = default)
        {
            var sql = BuildExistsQuery(tableName, column);
            var parameter = new SqlParameter("@value", value ?? DBNull.Value);
            var result = await ExecuteScalarAsync(CommandType.Text, sql, new[] { parameter }, cancellationToken).ConfigureAwait(false);
            return result != null;
        }

        public async Task<bool> IsExistAsync(string tableName, string whereClause, SqlParameter[] parameters, CancellationToken cancellationToken = default)
        {
            var sql = BuildExistsQueryWithWhere(tableName, whereClause);
            var result = await ExecuteScalarAsync(CommandType.Text, sql, parameters, cancellationToken).ConfigureAwait(false);
            return result != null;
        }

        // ------------------------------------------------------------- helpers

        /// <summary>Builds <c>SELECT TOP 1 1 FROM [table] WHERE [column] = @value</c>.</summary>
        internal static string BuildExistsQuery(string tableName, string column)
            => $"SELECT TOP 1 1 FROM {SqlIdentifier.Quote(tableName, nameof(tableName))} WHERE {SqlIdentifier.Quote(column, nameof(column))} = @value";

        /// <summary>
        /// Builds <c>SELECT TOP 1 1 FROM [table] WHERE (whereClause)</c>. The where clause is used as-is,
        /// so it must only contain parameter placeholders for any external values.
        /// </summary>
        internal static string BuildExistsQueryWithWhere(string tableName, string whereClause)
        {
            if (string.IsNullOrWhiteSpace(whereClause))
            {
                throw new ArgumentException("Where clause must not be empty.", nameof(whereClause));
            }
            return $"SELECT TOP 1 1 FROM {SqlIdentifier.Quote(tableName, nameof(tableName))} WHERE ({whereClause})";
        }

        private async Task<DataTable> ExecuteDataTableAsync(CommandType commandType, string commandText, SqlParameter[] parameters, CancellationToken cancellationToken)
        {
            using (var connection = new SqlConnection(_connectionString))
            using (var command = CreateCommand(connection, commandType, commandText, parameters))
            {
                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
                using (var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false))
                {
                    var table = new DataTable();
                    table.Load(reader);
                    return table;
                }
            }
        }

        private async Task<object> ExecuteScalarAsync(CommandType commandType, string commandText, SqlParameter[] parameters, CancellationToken cancellationToken)
        {
            using (var connection = new SqlConnection(_connectionString))
            using (var command = CreateCommand(connection, commandType, commandText, parameters))
            {
                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
                var result = await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
                return result is DBNull ? null : result;
            }
        }

        private async Task<int> ExecuteNonQueryAsync(CommandType commandType, string commandText, SqlParameter[] parameters, CancellationToken cancellationToken)
        {
            using (var connection = new SqlConnection(_connectionString))
            using (var command = CreateCommand(connection, commandType, commandText, parameters))
            {
                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
                return await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        private static SqlCommand CreateCommand(SqlConnection connection, CommandType commandType, string commandText, SqlParameter[] parameters)
        {
            if (string.IsNullOrWhiteSpace(commandText))
            {
                throw new ArgumentNullException(nameof(commandText));
            }

            var command = new SqlCommand(commandText, connection) { CommandType = commandType };
            SqlHelper.UpdateCommandTimeout(command);
            if (parameters != null)
            {
                foreach (var p in parameters)
                {
                    if (p == null) continue;
                    if ((p.Direction == ParameterDirection.Input || p.Direction == ParameterDirection.InputOutput) && p.Value == null)
                    {
                        p.Value = DBNull.Value;
                    }
                    command.Parameters.Add(p);
                }
            }
            return command;
        }
    }
}
