using System;
using System.Threading.Tasks;
using BlazorMarkedsplads.Models;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace BlazorMarkedsplads.Services
{
    public class UserRepository : IUserRepository
    {
        private readonly string _connectionString;

        public UserRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                                ?? throw new InvalidOperationException("DefaultConnection mangler i appsettings.");
        }

        public async Task<int> InsertAsync(User user)
        {
            const string sql = @"
            INSERT INTO users 
                (name, email, password, phone, address, city, zip_code)
            VALUES
                (@name, @mail, @pwd, @phone, @addr, @city, @zip)
            RETURNING id;
        ";

            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand(sql, conn);

            cmd.Parameters.AddWithValue("@name", user.Name);
            cmd.Parameters.AddWithValue("@mail", user.Email);
            cmd.Parameters.AddWithValue("@pwd", user.Password);    // Husk hash i produktion
            cmd.Parameters.AddWithValue("@phone", user.Phone ?? string.Empty);
            cmd.Parameters.AddWithValue("@addr", user.Address ?? string.Empty);
            cmd.Parameters.AddWithValue("@city", user.City ?? string.Empty);
            cmd.Parameters.AddWithValue("@zip", user.ZipCode ?? string.Empty);

            var result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            const string sql = "SELECT * FROM users WHERE email = @mail LIMIT 1;";

            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@mail", email);

            await using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new User
                {
                    Id = reader.GetInt32(reader.GetOrdinal("id")),
                    Name = reader.GetString(reader.GetOrdinal("name")),
                    Email = reader.GetString(reader.GetOrdinal("email")),
                    Password = reader.GetString(reader.GetOrdinal("password")),
                    Phone = reader.GetString(reader.GetOrdinal("phone")),
                    Address = reader.GetString(reader.GetOrdinal("address")),
                    City = reader.GetString(reader.GetOrdinal("city")),
                    ZipCode = reader.GetString(reader.GetOrdinal("zip_code"))
                };
            }

            return null;
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            const string sql = "SELECT * FROM users WHERE id = @uid;";

            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@uid", id);

            await using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new User
                {
                    Id = id,
                    Name = reader.GetString(reader.GetOrdinal("name")),
                    Email = reader.GetString(reader.GetOrdinal("email")),
                    Password = reader.GetString(reader.GetOrdinal("password")),
                    Phone = reader.GetString(reader.GetOrdinal("phone")),
                    Address = reader.GetString(reader.GetOrdinal("address")),
                    City = reader.GetString(reader.GetOrdinal("city")),
                    ZipCode = reader.GetString(reader.GetOrdinal("zip_code"))
                };
            }

            return null;
        }

        public async Task UpdateAsync(User user)
        {
            const string sql = @"
            UPDATE users 
            SET
                name     = @name,
                phone    = @phone,
                address  = @addr,
                city     = @city,
                zip_code = @zip
            WHERE id = @uid;
        ";

            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand(sql, conn);

            cmd.Parameters.AddWithValue("@name", user.Name);
            cmd.Parameters.AddWithValue("@phone", user.Phone ?? string.Empty);
            cmd.Parameters.AddWithValue("@addr", user.Address ?? string.Empty);
            cmd.Parameters.AddWithValue("@city", user.City ?? string.Empty);
            cmd.Parameters.AddWithValue("@zip", user.ZipCode ?? string.Empty);
            cmd.Parameters.AddWithValue("@uid", user.Id);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task UpdatePasswordAsync(int userId, string newPassword) // Renamed parameter
        {
            const string sql = "UPDATE users SET password = @pwd WHERE id = @uid;";

            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand(sql, conn);

            cmd.Parameters.AddWithValue("@pwd", newPassword); // Using the plain text password directly
            cmd.Parameters.AddWithValue("@uid", userId);

            await cmd.ExecuteNonQueryAsync();
        }
    }
}