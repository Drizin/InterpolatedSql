using Microsoft.Extensions.Configuration;

namespace InterpolatedSql.Dapper.Tests
{
    public class TestHelper
    {
        public static IConfiguration Configuration { get; set; }
        static TestHelper()
        {
            Configuration = new ConfigurationBuilder()
                .AddJsonFile("TestSettings.json")
                .Build();
        }
        public static string GetMSSQLConnectionString() => Configuration.GetConnectionString("MSSQLConnection");
        public static string GetPostgreSQLConnectionString() => Configuration.GetConnectionString("PostgreSQLConnection");
        public static string GetMySQLConnectionString() => Configuration.GetConnectionString("MySQLConnection");

    }
}
