using System;
using Xunit;

namespace Core.MSSQL.SqlHelper.Tests
{
    public class SqlIdentifierTests
    {
        [Theory]
        [InlineData("Users", "[Users]")]
        [InlineData("dbo.Users", "[dbo].[Users]")]
        [InlineData("[dbo].[My Table]", "[dbo].[My Table]")]
        [InlineData("[weird]]name]", "[weird]]name]")]
        [InlineData("_tmp#1", "[_tmp#1]")]
        [InlineData("db.dbo.Users", "[db].[dbo].[Users]")]
        public void Quote_AcceptsValidIdentifiers(string input, string expected)
        {
            Assert.Equal(expected, SqlIdentifier.Quote(input, "name"));
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("Users; DROP TABLE Users--")]
        [InlineData("Users WHERE 1=1")]
        [InlineData("Users'")]
        [InlineData("1Users")]
        [InlineData("dbo.")]
        [InlineData(".Users")]
        [InlineData("[unclosed")]
        [InlineData("[]")]
        [InlineData("a.b.c.d.e")]
        public void Quote_RejectsInvalidIdentifiers(string input)
        {
            Assert.Throws<ArgumentException>(() => SqlIdentifier.Quote(input, "name"));
        }
    }
}
