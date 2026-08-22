using Dapper;
using Npgsql;
using Microsoft.Extensions.Configuration;
using PeakWear.Core.DbModels;
using PeakWear.Core.Services;

namespace PeakWear.Data.Repositories;

public class UserRepository : IUserRepository
{
    private readonly string _connectionString;

    public UserRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Default")!;
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        const string sql = """
            SELECT id, email, password_hash, display_name, created_at_utc
            FROM users
            ORDER BY created_at_utc DESC
            """;

        await using var connection = new NpgsqlConnection(_connectionString);
        return await connection.QueryAsync<User>(sql);
    }
}