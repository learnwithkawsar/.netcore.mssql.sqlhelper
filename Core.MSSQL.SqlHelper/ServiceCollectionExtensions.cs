using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.MSSQL.SqlHelper
{
    public static class ServiceCollectionExtensions
    {
        public static void RegisterSqlHelper(this IServiceCollection services, string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

           

           // services.AddScoped<IInternalExceptionService, ExceptionService>(sp => new ExceptionService(connectionString));
            services.AddScoped<ISqlHelperService, SqlHelperService>(sp => new SqlHelperService(connectionString));
        }
    }
}
