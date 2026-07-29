using System.Data;

namespace StayOps.Application.Common.Interfaces;

/// <summary>Creates raw ADO.NET connections for Dapper-based stored-procedure calls and reports.</summary>
public interface IDapperConnectionFactory
{
    IDbConnection CreateConnection();
}
