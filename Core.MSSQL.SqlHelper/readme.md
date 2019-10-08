# Core.MSSQL.SqlHelper
Configuring Core.MSSQL.SqlHelper into your ASP.NET Core project is as simple as below:

First install the Core.MSSQL.SqlHelper nuget package into your project as follows:

> Install-Package Core.MSSQL.SqlHelper

 Then in your startup class register the Core.MSSQL.SqlHelper as follows:

            var connectionString = Configuration.GetConnectionString("LocalDbConnection");
            services.RegisterSqlHelper(connectionString);

After that you can use in your controllers or others 

       
         private readonly ISqlHelperService _sqlHelperService;
        public DemoController(SqlHelperService sqlHelperService)
        {          
            _sqlHelperService = sqlHelperService;