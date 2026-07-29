using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using StayOps.Application.Common.Interfaces;

namespace StayOps.Infrastructure.Persistence;

public class DapperConnectionFactory(IConfiguration configuration) : IDapperConnectionFactory
{
    private readonly string _connectionString = configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

    public IDbConnection CreateConnection() => new SqlConnection(_connectionString);
}
