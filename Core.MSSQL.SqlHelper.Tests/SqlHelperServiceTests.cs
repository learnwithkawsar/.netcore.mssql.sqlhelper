using System;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Core.MSSQL.SqlHelper.Tests
{
    public class SqlHelperServiceTests
    {
        [Fact]
        public void BuildExistsQuery_QuotesIdentifiersAndParameterizesValue()
        {
            var sql = SqlHelperService.BuildExistsQuery("dbo.Users", "Email");
            Assert.Equal("SELECT TOP 1 1 FROM [dbo].[Users] WHERE [Email] = @value", sql);
        }

        [Fact]
        public void BuildExistsQuery_RejectsInjectedColumn()
        {
            Assert.Throws<ArgumentException>(() => SqlHelperService.BuildExistsQuery("Users", "Email = '' OR 1=1 --"));
        }

        [Fact]
        public void BuildExistsQueryWithWhere_WrapsClause()
        {
            var sql = SqlHelperService.BuildExistsQueryWithWhere("Users", "Email = @email AND IsActive = 1");
            Assert.Equal("SELECT TOP 1 1 FROM [Users] WHERE (Email = @email AND IsActive = 1)", sql);
        }

        [Fact]
        public void BuildExistsQueryWithWhere_RejectsEmptyClause()
        {
            Assert.Throws<ArgumentException>(() => SqlHelperService.BuildExistsQueryWithWhere("Users", " "));
        }

        [Fact]
        public void Constructor_RejectsEmptyConnectionString()
        {
            Assert.Throws<ArgumentNullException>(() => new SqlHelperService(""));
        }

        [Fact]
        public void RegisterSqlHelper_ResolvesInterfaceAndConcreteAsSameInstance()
        {
            var provider = new ServiceCollection()
                .RegisterSqlHelper("Server=.;Database=Test;Trusted_Connection=True;")
                .BuildServiceProvider();

            var byInterface = provider.GetRequiredService<ISqlHelperService>();
            var byClass = provider.GetRequiredService<SqlHelperService>();
            Assert.Same(byInterface, byClass);
        }

        [Fact]
        public void RegisterSqlHelper_RejectsEmptyConnectionString()
        {
            Assert.Throws<ArgumentNullException>(() => new ServiceCollection().RegisterSqlHelper(null));
        }
    }
}
