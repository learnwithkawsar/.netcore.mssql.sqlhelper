using System;
using Microsoft.Extensions.DependencyInjection;

namespace Core.MSSQL.SqlHelper
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers <see cref="ISqlHelperService"/> (and <see cref="SqlHelperService"/>) as singletons
        /// using the given SQL Server connection string.
        /// </summary>
        public static IServiceCollection RegisterSqlHelper(this IServiceCollection services, string connectionString)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            var instance = new SqlHelperService(connectionString);
            services.AddSingleton(instance);
            services.AddSingleton<ISqlHelperService>(instance);
            return services;
        }
    }
}
