using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace ConfigApi.Context.Factory;

public interface IDbConnectionFactory
{
    IDbConnection Create();
}

public class DbConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public DbConnectionFactory(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Connection string bulunamadı.");
    }

    public IDbConnection Create() => new SqlConnection(_connectionString);
}
