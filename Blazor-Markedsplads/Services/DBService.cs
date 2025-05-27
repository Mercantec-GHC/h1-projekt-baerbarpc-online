using Npgsql;
using Microsoft.Extensions.Configuration;
using BlazorMarkedsplads.Models;

namespace BlazorMarkedsplads.Services
{
    public class DBService
    {
        private readonly string _connectionString;

        public DBService(IConfiguration configuration)
            => _connectionString = configuration.GetConnectionString("DefaultConnection");

        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                await using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        // --- USERS ----------------------------------------------------

        public async Task<string> SetupUserTable()
        {
            var sql = @"
                CREATE TABLE IF NOT EXISTS users (
                    id         SERIAL PRIMARY KEY,
                    name       VARCHAR(255),
                    email      VARCHAR(255),
                    password   VARCHAR(255),
                    phone      VARCHAR(255),
                    address    VARCHAR(255),
                    city       VARCHAR(255),
                    zip_code   VARCHAR(255)
                );";

            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand(sql, conn);
            await cmd.ExecuteNonQueryAsync();

            return "User table created successfully";
        }

        public async Task<List<User>> GetAllUsers()
        {
            var list = new List<User>();
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand("SELECT * FROM users", conn);
            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                list.Add(new User
                {
                    Id = reader.GetInt32(reader.GetOrdinal("id")),
                    Name = reader.GetString(reader.GetOrdinal("name")),
                    Email = reader.GetString(reader.GetOrdinal("email")),
                    Password = reader.GetString(reader.GetOrdinal("password")),
                    Phone = reader.GetString(reader.GetOrdinal("phone")),
                    Address = reader.GetString(reader.GetOrdinal("address")),
                    City = reader.GetString(reader.GetOrdinal("city")),
                    ZipCode = reader.GetString(reader.GetOrdinal("zip_code"))
                });
            }

            return list;
        }


        // --- PRODUCTMODELS --------------------------------------------

        public async Task<string> SetupProductModelsTable()
        {
            var sql = @"
                CREATE TABLE IF NOT EXISTS product_models (
                    id           SERIAL PRIMARY KEY,
                    brand        VARCHAR(255),
                    model        VARCHAR(255),
                    gpu          VARCHAR(255),
                    cpu          VARCHAR(255),
                    ram          VARCHAR(255),
                    storage      VARCHAR(255),
                    os           VARCHAR(255),
                    price        VARCHAR(255),
                    screen_size  VARCHAR(255),
                    condition    VARCHAR(255)
                );";

            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand(sql, conn);
            await cmd.ExecuteNonQueryAsync();

            return "ProductModels table created successfully";
        }

        public async Task<List<ProductModel>> GetAllProductModelsAsync()
        {
            var list = new List<ProductModel>();

            try
            {
                const string sql = "SELECT * FROM product_models;";
                await using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();
                await using var cmd = new NpgsqlCommand(sql, conn);
                await using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    // bemærk lille l  ↓ ↓
                    list.Add(new ProductModel
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("id")),
                        Brand = reader.GetString(reader.GetOrdinal("brand")),
                        Model = reader.GetString(reader.GetOrdinal("model")),
                        Gpu = reader.GetString(reader.GetOrdinal("gpu")),
                        Cpu = reader.GetString(reader.GetOrdinal("cpu")),
                        Ram = reader.GetString(reader.GetOrdinal("ram")),
                        Storage = reader.GetString(reader.GetOrdinal("storage")),
                        OS = reader.GetString(reader.GetOrdinal("os")),
                        Price = reader.GetString(reader.GetOrdinal("price")),
                        ScreenSize = reader.GetString(reader.GetOrdinal("screen_size")),
                        Condition = reader.GetString(reader.GetOrdinal("condition"))
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching product models: {ex.Message}");
                // evt. _logger.LogError(ex, "...");
            }

            return list;        
        }
        // Henter de dyreste laptops (default 4)
        public async Task<List<ProductModel>> GetPremiumProductModelsAsync(int take = 4)
        {
            var list = new List<ProductModel>();

            const string sql = @"
        SELECT *
        FROM   product_models
        ORDER  BY (regexp_replace(price, '[^0-9]', '', 'g'))::int DESC   -- cast ”12 499 kr” → 12499
        LIMIT  @take;";

            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("take", take);

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new ProductModel
                {
                    Id = reader.GetInt32(reader.GetOrdinal("id")),
                    Brand = reader.GetString(reader.GetOrdinal("brand")),
                    Model = reader.GetString(reader.GetOrdinal("model")),
                    Gpu = reader.GetString(reader.GetOrdinal("gpu")),
                    Cpu = reader.GetString(reader.GetOrdinal("cpu")),
                    Ram = reader.GetString(reader.GetOrdinal("ram")),
                    Storage = reader.GetString(reader.GetOrdinal("storage")),
                    OS = reader.GetString(reader.GetOrdinal("os")),
                    Price = reader.GetString(reader.GetOrdinal("price")),
                    ScreenSize = reader.GetString(reader.GetOrdinal("screen_size")),
                    Condition = reader.GetString(reader.GetOrdinal("condition"))
                });
            }

            return list;
        }

        // Fritekstsøgning (brand, model, cpu, gpu, os …)
        public async Task<List<ProductModel>> SearchProductModelsAsync(string term)
        {
            var list = new List<ProductModel>();

            const string sql = @"
        SELECT *
        FROM   product_models
        WHERE  brand  ILIKE @t
           OR  model  ILIKE @t
           OR  cpu    ILIKE @t
           OR  gpu    ILIKE @t
           OR  os     ILIKE @t;";

            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("t", $"%{term}%");

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new ProductModel
                {
                    Id = reader.GetInt32(reader.GetOrdinal("id")),
                    Brand = reader.GetString(reader.GetOrdinal("brand")),
                    Model = reader.GetString(reader.GetOrdinal("model")),
                    Gpu = reader.GetString(reader.GetOrdinal("gpu")),
                    Cpu = reader.GetString(reader.GetOrdinal("cpu")),
                    Ram = reader.GetString(reader.GetOrdinal("ram")),
                    Storage = reader.GetString(reader.GetOrdinal("storage")),
                    OS = reader.GetString(reader.GetOrdinal("os")),
                    Price = reader.GetString(reader.GetOrdinal("price")),
                    ScreenSize = reader.GetString(reader.GetOrdinal("screen_size")),
                    Condition = reader.GetString(reader.GetOrdinal("condition"))
                });
            }

            return list;
        }
    }
}

