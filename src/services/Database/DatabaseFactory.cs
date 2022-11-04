using System.Data.SqlClient;
namespace services.Database;

public static class DatabaseFactory
{
    private static Lazy<string> _connectionString = new (() =>
    {
        var connectionStringBuilder = new SqlConnectionStringBuilder(Utils.DbConnectionString)
        {
            Pooling = true,
            ApplicationName = typeof(DatabaseFactory).Assembly.FullName
        };

        return connectionStringBuilder.ConnectionString;
    });
    
    public static SqlConnection GetConnection()
    {
        var connection = new SqlConnection(_connectionString.Value);

        return connection;
    }
}
